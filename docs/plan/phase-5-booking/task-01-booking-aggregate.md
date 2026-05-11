# Task `P5-T01` — Booking Aggregate + State Machine

> **Phase**: 5 — Booking System
> **Estimated size**: L
> **Depends on**: Phase 4 complete
> **Can run in parallel with**: nothing

---

## 1. Context

The `Booking` aggregate ties a Player to a Training. Concurrency is the hard part: two players hit "Book" at the same instant on the last open slot. We design the aggregate around explicit state transitions plus DB-level guarantees (unique indexes + serializable check) so over-booking is impossible.

## 2. Goal

> A booking is created, confirmed, waitlisted, or cancelled with deterministic rules; concurrent attempts to grab the last slot result in **one** Confirmed and **one** either WaitingList or rejected.

## 3. Scope

### In scope

- Aggregate `Booking` in `Domain/Bookings/`:
  - `Id`, `TenantId`, `TrainingId`, `PlayerMemberId`, `BookedByMemberId` (the user who clicked book — for parent-on-behalf-of-child), `Status` (`Confirmed | WaitingList | Cancelled`), `BookedAt`, `CancelledAt?`, `CancellationReason?`, `LateCancel` (bool), `WaitlistPosition?` (1-based), `Version`.
  - Invariants:
    - One **non-Cancelled** booking per `(TrainingId, PlayerMemberId)`.
    - `WaitlistPosition` set iff `Status=WaitingList`.
- Use cases:
  - `CreateBooking(TrainingId, PlayerMemberId, BookedByMemberId, IdempotencyKey)`:
    - Validates Training is `Published` and within booking window (T03).
    - Validates actor can act on behalf of player (`IResourceAuthorizationService.CanActOnBehalfOf`).
    - Counts current Confirmed bookings; if `< Capacity` → Confirmed; else if waitlist enabled → WaitingList with `position = max(position)+1`; else → 422.
    - Idempotency-Key support (see API conventions).
  - `CancelBooking(BookingId, Reason?)`:
    - Status transitions Confirmed/WaitingList → Cancelled.
    - Sets `LateCancel=true` if `now > training.StartUtc - ClubSettings.DefaultCancellationWindowHours`.
    - Triggers promotion (P5-T02) if the cancelled booking was Confirmed.
    - Re-issues `WaitlistPosition` for any WaitingList bookings that were promoted or shifted.
- Concurrency strategy:
  - DB unique index `(TrainingId, PlayerMemberId) WHERE Status <> 'Cancelled'` prevents duplicates.
  - Transaction isolation **READ COMMITTED** + an explicit `SELECT FOR UPDATE` (or Postgres advisory lock keyed by `TrainingId`) inside the transaction during booking to serialize per-training booking attempts.
  - Document the choice and the reasoning.
- Endpoints:
  - `POST /api/v1/trainings/{trainingId}/bookings` body `{ playerMemberId }`.
  - `GET /api/v1/trainings/{trainingId}/bookings` (Coach+ for that training; HeadCoach+ in general).
  - `GET /api/v1/me/bookings` (self) and `GET /api/v1/members/{id}/bookings` (Parent for children, Coach+ for any).
  - `DELETE /api/v1/bookings/{bookingId}` — soft cancel.
- Audit: `booking.created`, `booking.confirmed`, `booking.waitlisted`, `booking.cancelled`, `booking.late_cancelled`.

### Out of scope

- Waiting-list promotion mechanics in detail → **P5-T02**.
- Booking/cancellation window validation logic → **P5-T03**.
- Outbox / event publication → **P5-T05**.
- Frontend → **P5-T06**.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Bookings/Booking.cs`, `BookingStatus.cs`.
- EF config + migration `AddBookings` with the unique-where-not-cancelled index and indexes for `(TrainingId, Status, WaitlistPosition)`.
- Application use cases + endpoints.
- `IBookingRepository` with the per-training lock helper.
- Idempotency middleware reading `Idempotency-Key` header for `POST` mutations (general infrastructure introduced here; applied to bookings now and other writes later if needed).
- Tests:
  - Concurrency: a race that spawns 10 booking attempts on a training with capacity 3 → exactly 3 Confirmed and 7 WaitingList (or 0 WaitingList if disabled, with the rest 422).
  - Duplicate booking (same player same training) → 409.
  - Idempotent retry with same Idempotency-Key → same response, no duplicate.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] A booking on a non-full Published training returns `201 { status: "Confirmed" }`.
- [ ] A booking on a full training with waitlist enabled returns `201 { status: "WaitingList", waitlistPosition: N }`.
- [ ] A booking on a full training with waitlist disabled returns `422` with `type=slotsmart/errors/training-full`.
- [ ] A Player booking a child without the parent relation gets `403`.
- [ ] Cancelling a Confirmed booking returns `204` and (per P5-T02) potentially promotes a waitlister.
- [ ] Concurrency test: 10 parallel POSTs with capacity 3 produce exactly 3 Confirmed.
- [ ] Idempotency: retrying the same POST with the same `Idempotency-Key` returns the original response.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.
- [ ] DB index `EXPLAIN` shows the unique-where-not-cancelled index used.

## 8. Handoff notes / gotchas

- The Postgres advisory lock approach: `pg_advisory_xact_lock(hashtext('booking:' || training_id::text))` — released at commit. Document.
- Alternatively, `SELECT id, version FROM trainings WHERE id = … FOR UPDATE` works but locks the row hard; advisory locks are nicer.
- The unique index needs to be a **partial** index on Postgres (`WHERE status <> 'Cancelled'`). EF Core's `HasIndex().IsUnique().HasFilter("\"Status\" <> 'Cancelled'")`.
- Idempotency middleware stores `(tenant, idempotency-key, request-hash, response)` for 24h in a `IdempotencyKeys` table.

## 9. Suggested execution outline

1. Aggregate + invariants + tests (Domain only, no DB).
2. Migration + repository.
3. Concurrency strategy with integration test (Testcontainers + parallel HttpClient calls).
4. Idempotency middleware + table + migration.
5. Endpoints + audit.
6. CHANGELOG.

## 10. Open questions / risks

- Question: do we expose Confirmed bookings to other Players (privacy)? **Decision (MVP)**: only count is public; names are visible to the training's coach + Club Admin. Document.
- Risk: with advisory locks, long-running transactions could serialize too much. **Mitigation**: keep the booking transaction tiny — only the count + insert + audit.
