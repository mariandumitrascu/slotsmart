# Task `P5-T05` — Outbox + Domain Events for Bookings

> **Phase**: 5 — Booking System
> **Estimated size**: M
> **Depends on**: P5-T01
> **Can run in parallel with**: P5-T02, P5-T03

---

## 1. Context

Notifications (Phase 6) and future integrations (analytics, payments) need a **reliable** way to be told about domain events. We implement the **transactional outbox** pattern: events are persisted in the same transaction as the state change, and a background relay dispatches them to in-process handlers (and later, to external consumers).

## 2. Goal

> Booking events (`BookingConfirmed`, `BookingPromoted`, `BookingCancelled`) are reliably emitted via the outbox; a test consumer observes each event exactly once even under crash-during-dispatch conditions.

## 3. Scope

### In scope

- Table `OutboxMessages`:
  - `Id` (UUIDv7), `TenantId`, `OccurredAt`, `Type` (e.g. `"BookingConfirmed"`), `Payload` (jsonb), `ProcessedAt?`, `Attempts`, `LastError?`.
- An `IDomainEventPublisher` abstraction; the default implementation **writes to the outbox** in the current `DbContext`.
- A SaveChanges interceptor that drains the `DomainEvents` collection from each modified aggregate into outbox rows before commit.
- Background `OutboxDispatcher` Quartz job:
  - Polls every 2s for unprocessed rows (configurable).
  - Dispatches via MediatR to in-process handlers; on success sets `ProcessedAt`.
  - On failure increments `Attempts`, stores `LastError`, backs off (exponential with cap).
  - After 10 failed attempts moves to a dead-letter view (filter `Attempts > 10`).
- Define the three event types as `record`s in `Domain/Bookings/Events/`:
  - `BookingConfirmed(BookingId, TrainingId, PlayerMemberId, BookedByMemberId)`
  - `BookingPromoted(BookingId, TrainingId, PlayerMemberId)`
  - `BookingCancelled(BookingId, TrainingId, PlayerMemberId, LateCancel, CancelledByMemberId)`
- Wire these into the booking aggregate methods.
- Add a **null** notification handler in `Application/Features/Notifications/EventHandlers/` that just logs at Information — Phase 6 fills in the real one.

### Out of scope

- External transport (Kafka, RabbitMQ) → V2.
- Saga / process manager → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md) (mentions outbox under Infrastructure)
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `backend/src/SlotSmart.Infrastructure/Outbox/OutboxMessage.cs`, `OutboxDispatcher.cs`.
- Migration `AddOutboxMessages` with indexes on `(ProcessedAt, OccurredAt)` partial where unprocessed.
- `IDomainEventPublisher` abstraction + EF implementation.
- SaveChanges interceptor `DomainEventsToOutboxInterceptor`.
- Quartz registration.
- Events records in `Domain/Bookings/Events/`.
- Tests:
  - Crash before dispatch: events remain in outbox; on restart they are processed.
  - Dispatch failure: row marked Attempts=N with LastError; retries with backoff; eventually dead-lettered.
  - Idempotent in-process handler: re-processing the same event row twice is safe (the test handler is written to be idempotent).
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Confirming a booking writes a `BookingConfirmed` row to outbox in the same transaction.
- [ ] The dispatcher picks up the row within 2 seconds and marks it processed; an in-process listener sees the event.
- [ ] Killing the process between insert and dispatch leaves the row unprocessed; restarting picks it up.
- [ ] A handler that throws causes `Attempts` to increment; after 10 failures the row is in dead-letter state.
- [ ] Tenant filter applies to the outbox: tenants don't see each other's messages.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated.
- [ ] A small admin endpoint `GET /api/v1/admin/outbox?status=dead` (ClubAdmin) for ops visibility.

## 8. Handoff notes / gotchas

- The dispatcher must avoid the classic "polling thundering herd" — use `SKIP LOCKED` in the SELECT to allow multiple dispatcher instances safely: `SELECT … FROM outbox_messages WHERE processed_at IS NULL ORDER BY occurred_at FOR UPDATE SKIP LOCKED LIMIT N`.
- Serialize event payloads with `System.Text.Json` and include a `Schema` field if you anticipate evolving payloads.

## 9. Suggested execution outline

1. Table + migration.
2. Interceptor + publisher.
3. Dispatcher + Quartz job.
4. Event records + emit from booking handlers.
5. Tests for the failure modes.
6. CHANGELOG.

## 10. Open questions / risks

- Risk: the dispatcher poll interval vs notification latency — 2s is fine for email, may need shorter for SMS in V2. Make configurable.
