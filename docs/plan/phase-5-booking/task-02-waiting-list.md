# Task `P5-T02` — Waiting List + Auto-Promotion

> **Phase**: 5 — Booking System
> **Estimated size**: M
> **Depends on**: P5-T01
> **Can run in parallel with**: P5-T03, P5-T05

---

## 1. Context

Phase 5-T01 introduces the WaitingList status. This task makes the waiting list **work**: positions are stable, promotions are atomic, and the rules around auto-promotion vs manual promotion are clear.

## 2. Goal

> When a Confirmed booking is cancelled and `ClubSettings.AutoPromoteFromWaitingList=true`, the WaitingList head atomically becomes Confirmed (within the same transaction as the cancellation). Positions stay 1-based and contiguous.

## 3. Scope

### In scope

- Auto-promotion in the `CancelBooking` handler:
  - Under the same advisory lock used for booking, after marking the cancelled row, find the WaitingList booking with the smallest `WaitlistPosition` for this training.
  - Flip its `Status` to Confirmed, clear `WaitlistPosition`.
  - Decrement positions of subsequent waiters so positions stay contiguous (recompute positions in one statement).
  - Emit a `BookingPromoted` domain event (P5-T05 picks it up).
- Manual promotion: `POST /api/v1/bookings/{id}:promote` (Coach+ for that training, or HeadCoach+) — allowed when auto-promote is disabled or when an admin wants to force.
- Waitlist viewing:
  - `GET /api/v1/trainings/{trainingId}/waitlist` — returns ordered list (Coach+ for that training; HeadCoach+ generally).
- Waitlist reorder: `PUT /api/v1/trainings/{trainingId}/waitlist/order` (HeadCoach+) accepts the desired order of booking ids; validates membership, then renumbers atomically.
- Cancellation by a WaitingList booking: just remove and renumber.

### Out of scope

- "Notify when promoted" → Phase 6 (uses the event).
- Time-bound auto-decline ("accept within 4 hours or lose your spot") → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)

## 5. Deliverables

- Extend `CancelBooking` handler with promotion path.
- New use cases `PromoteBooking`, `ReorderWaitlist`, `GetWaitlist`.
- Endpoints in `BookingsEndpoints.cs`.
- Domain event `BookingPromoted` (handled by outbox in P5-T05).
- Tests:
  - Single cancellation promotes head; positions renumber.
  - Concurrent cancellation + new booking attempt: only one wins the freed slot (the promoted waiter), the new booking goes to waitlist tail.
  - Manual reorder with stale order returns 409 (the list changed under us).
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Cancelling a Confirmed booking in a training with non-empty waitlist promotes the head and renumbers contiguously.
- [ ] If `AutoPromoteFromWaitingList=false`, the slot is left open and `POST :promote` is required.
- [ ] Reorder enforces "all current waitlist ids included exactly once".
- [ ] Concurrency: promotion and new booking are serialized correctly (per-training advisory lock).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Renumbering positions can be one SQL statement: `UPDATE bookings SET waitlist_position = waitlist_position - 1 WHERE training_id = ? AND status = 'WaitingList' AND waitlist_position > <promoted-position>`.
- The `BookingPromoted` event must include both the booking id and the training id so notifications can be composed in Phase 6.

## 9. Suggested execution outline

1. Promotion logic inside `CancelBooking` (still under the per-training lock).
2. Manual promote + reorder commands + tests.
3. Event emission stub (handler is in P5-T05).
4. CHANGELOG.

## 10. Open questions / risks

- Question: cap waitlist size? **Decision (MVP)**: no cap; revisit if a single training gets thousands of waiters.
