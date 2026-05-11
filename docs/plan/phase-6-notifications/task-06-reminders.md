# Task `P6-T06` — Scheduled Reminders

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: M
> **Depends on**: P6-T05
> **Can run in parallel with**: P6-T07

---

## 1. Context

Reminders are notifications triggered by **time**, not by an event. Examples: "Your training starts in 24 hours."

## 2. Goal

> A scheduled job emits a `TrainingReminder` notification at T-24h (configurable) for every booked Player on each upcoming training, exactly once per training.

## 3. Scope

### In scope

- Job `TrainingReminderJob` (Quartz):
  - Runs every 15 minutes.
  - For each tenant: find trainings starting in the next reminder window (`now + reminderOffsetHours ± 7.5min`) whose `RemindersSentAt` is null.
  - For each such training, send `TrainingReminder` notification to all current Confirmed bookers (+ parents per the rules in P6-T05).
  - Stamp `Training.RemindersSentAt = now` to prevent duplicates.
- Reminder offset:
  - `ClubSettings.ReminderHoursBeforeStart` (default 24).
  - Per-training override `Training.ReminderHoursBeforeStartOverride?`.
  - `0` disables reminders for a training.
- Reminders never resend after a training has been cancelled.

### Out of scope

- Multiple reminders per training (e.g. T-24h AND T-2h) → V2.
- SMS reminders → V2.

## 4. Inputs

- Architecture docs: same as P6-T05.

## 5. Deliverables

- Migration: add `RemindersSentAt` to `Trainings`, `ReminderHoursBeforeStart` to club settings + override on `Training`.
- Quartz job + tests:
  - A training starting in 23.5h is sent reminders when the job runs.
  - The same training is not reminded twice.
  - Disabled reminders (`0`) are skipped.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] A test that sets `IClock` to a fixed instant and a training at +24h emits reminders for all bookers + parents on first job tick.
- [ ] A second tick at the same instant does not emit duplicates.
- [ ] A training cancelled before the reminder window does not emit.
- [ ] Per-training override (`override=2`) correctly fires at T-2h.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated.
- [ ] OpenAPI updated for the override field.

## 8. Handoff notes / gotchas

- Use the `±7.5min` half-window so a 15-min cadence covers any training without slipping. Adjust if cadence changes.
- `RemindersSentAt` is a per-training stamp, not per-recipient — the dispatcher path already handles per-recipient delivery.

## 9. Suggested execution outline

1. Migration.
2. Job + tests with `IClock`.
3. CHANGELOG.

## 10. Open questions / risks

- Risk: time-zone confusion. **Mitigation**: do everything in UTC; the override is hours, not local times.
