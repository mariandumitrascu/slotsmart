# Task `P5-T04` — Booking ↔ Attendance Integration

> **Phase**: 5 — Booking System
> **Estimated size**: S
> **Depends on**: P5-T01, P4-T05
> **Can run in parallel with**: P5-T02, P5-T03, P5-T05

---

## 1. Context

In Phase 4, "expected players" had to be added manually because bookings didn't exist. Now they do. We wire confirmed bookings to be the source of expected attendees.

## 2. Goal

> Confirming a booking adds the player as `Expected` in attendance; cancelling a confirmed booking removes them from Expected (only). Promotion from waitlist also adds Expected.

## 3. Scope

### In scope

- A domain-event handler subscribed to `BookingConfirmed` and `BookingPromoted` that calls `AddExpectedPlayers` on the training.
- A handler subscribed to `BookingCancelled` (for Confirmed → Cancelled transitions only) that calls `RemoveExpectedPlayer` — **only if** the existing `AttendanceRecord.Status` is `Expected`. If a coach already marked Present/Late/Absent/Excused, leave it alone (the cancellation is "late" after attendance was recorded).
- Make these handlers run in the same transaction as the booking change (so attendance is always consistent), via MediatR pipeline or `IDomainEventDispatcher` after `SaveChangesAsync`.
- Remove the manual "Add Expected" endpoint from P4-T05's public surface (keep the use case internal for tests / future bulk import).

### Out of scope

- Bulk import of expected players via CSV → V2.
- Coach-driven manual expected list overrides → not needed; bookings are the source.

## 4. Inputs

- API contracts from P4-T05.

## 5. Deliverables

- Event handlers in `Application/Features/Attendance/EventHandlers/`.
- Endpoint surface adjustment in `AttendanceEndpoints.cs`.
- Tests:
  - Confirm booking → attendance row Expected appears.
  - Cancel before any marking → attendance row removed.
  - Cancel after coach marked Present → attendance untouched, booking `lateCancel=true`.
  - Waitlist promotion → attendance row Expected appears.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Booking confirmation creates an Expected attendance row.
- [ ] Cancellation of a Confirmed booking before any marking removes the Expected row.
- [ ] Cancellation after the player was marked Present leaves attendance untouched.
- [ ] Promotion from waitlist creates Expected.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Domain events are dispatched **after** `SaveChangesAsync` but **inside** the unit-of-work in our setup; if you use an outbox (P5-T05) for external events, **attendance must NOT** go through the outbox — it's a same-transaction concern. Document the split clearly.

## 9. Suggested execution outline

1. Implement handlers + tests.
2. Adjust attendance endpoints.
3. CHANGELOG.

## 10. Open questions / risks

- none significant.
