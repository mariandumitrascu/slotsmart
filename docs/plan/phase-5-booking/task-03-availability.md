# Task `P5-T03` — Availability & Booking-Window Enforcement

> **Phase**: 5 — Booking System
> **Estimated size**: M
> **Depends on**: P5-T01
> **Can run in parallel with**: P5-T02, P5-T05

---

## 1. Context

Bookings have rules around **when** they can happen: a training opens for booking some time before its start (lead time), closes some time before it starts (close time), and cancellations are "late" past a configured window. These rules live in `ClubSettings` (Phase 3) with optional per-training overrides.

## 2. Goal

> Bookings outside the booking window are rejected with a clear error. Cancellations are always allowed but are flagged `lateCancel=true` past the cancellation window.

## 3. Scope

### In scope

- Per-training overrides on `Training`:
  - `BookingOpensAtUtc?` (defaults to `StartUtc - LeadTime` when null).
  - `BookingClosesAtUtc?` (defaults to `StartUtc - CloseOffsetHours`, default close offset = 2h).
  - `CancellationWindowHoursOverride?`.
  - `WaitingListEnabledOverride?`.
- `ClubSettings` provides:
  - `DefaultBookingLeadTimeHours` (when the window opens before start).
  - `DefaultBookingCloseOffsetHours` (when it closes — new field; default 2).
  - `DefaultCancellationWindowHours`.
  - `WaitingListEnabledByDefault`.
- Validation in `CreateBooking`:
  - If `now < BookingOpensAtUtc` → 422 `slotsmart/errors/booking-not-yet-open`.
  - If `now > BookingClosesAtUtc` → 422 `slotsmart/errors/booking-closed`.
  - If Training is `Cancelled` → 409.
- Validation in `CancelBooking`:
  - Always allowed for the booker. If `now > StartUtc - CancellationWindowHours` → `lateCancel=true` and audit `booking.late_cancelled`.
  - Cancelling AFTER training start is allowed and counts as `lateCancel`; Coach+ can still mark Absent in attendance.

### Out of scope

- Penalty system for late cancellation → V2.
- Per-member booking quotas ("max 2 bookings per week") → V2.

## 4. Inputs

- Architecture docs: club-settings extension from P3-T01.

## 5. Deliverables

- Migration: add the four nullable override columns to `Trainings`; add `DefaultBookingCloseOffsetHours` to club settings.
- Update `Training` aggregate with new fields + invariant: `BookingOpensAtUtc < BookingClosesAtUtc <= StartUtc`.
- Update `CreateBooking` and `CancelBooking` validators / handlers.
- New endpoint `PATCH /api/v1/trainings/{id}/booking-policy` (HeadCoach+) for the overrides.
- Tests for each window error type; late-cancel flag set correctly.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Booking opens at `StartUtc - LeadTime` and closes at `StartUtc - CloseOffset` by default.
- [ ] Setting a per-training override changes the effective window.
- [ ] Booking before open or after close returns the documented 422 codes.
- [ ] Cancelling within window: `lateCancel=false`; outside: `lateCancel=true`; in both cases the booking is cancelled.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Read `IClock.UtcNow` once at the top of each handler; otherwise tests are flaky.
- Surface the effective window in `GET /api/v1/trainings/{id}` so the frontend can disable the Book button when closed without an extra call.

## 9. Suggested execution outline

1. Migration + field defaults.
2. Aggregate update + invariants + tests.
3. Validator + handler updates.
4. New override endpoint.
5. CHANGELOG.

## 10. Open questions / risks

- Question: what timezone for the window cutoffs in the UI? **Decision**: UTC stored; UI shows in club timezone (matches calendar).
