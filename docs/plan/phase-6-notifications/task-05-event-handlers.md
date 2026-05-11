# Task `P6-T05` — Booking & Training Event Handlers

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: M
> **Depends on**: P6-T01..T04
> **Can run in parallel with**: P6-T07

---

## 1. Context

Domain events from Phases 4 and 5 (`BookingConfirmed`, `BookingCancelled`, `BookingPromoted`, `TrainingCancelled`, `TrainingChanged`, `MemberInvited`) need to become notifications. This task wires the handlers.

## 2. Goal

> Each listed event, when dispatched by the outbox, produces a `NotificationRequest` with the right category, recipient list, and data. Tests assert the recipient lists.

## 3. Scope

### In scope

- Event handlers in `Application/Features/Notifications/EventHandlers/`:
  - `BookingConfirmedHandler` → notify `recipient = bookedPlayer + parents-of-player`.
  - `BookingCancelledHandler` → notify `bookedPlayer + parents` AND the training's coach (if cancelled by player).
  - `BookingPromotedHandler` → notify the promoted player + parents (special category: `WaitlistPromoted`).
  - `TrainingCancelledHandler` → notify all current Confirmed + WaitingList bookers + their parents.
  - `TrainingChangedHandler` (e.g. time/coach changed) → same recipient set as cancelled, with category `TrainingChanged` and a `changes` payload.
  - `MemberInvitedHandler` → notify the invitee with category `InvitationSent` (Email only; in-app would require an existing account).
- Recipient resolution helper `IRecipientResolver` (under `Application/Features/Notifications/Recipients/`):
  - `ForPlayer(playerMemberId)` → returns `[playerUserId, parentsUserIds]` minus deactivated.
  - `ForTrainingBookings(trainingId)` → resolves the booker + their parents.
- Payloads are kept small but stable: include ids the templates need to render (training date, title, location, etc.). The renderer can re-fetch detail if it needs more.

### Out of scope

- Reminders (scheduled events) → **P6-T06**.

## 4. Inputs

- Domain events from Phase 4 + 5.

## 5. Deliverables

- Handlers + tests.
- `IRecipientResolver` + EF impl + tests.
- Update OpenAPI examples if any new admin endpoints land.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Confirming a booking for a child Player results in notifications to the child AND all their active Parents.
- [ ] Cancelling a training notifies all Confirmed + WaitingList bookers + their Parents.
- [ ] Promotion creates `WaitlistPromoted` notifications.
- [ ] Member invitation triggers an email-only invitation notification (the invitee has no in-app yet).
- [ ] Cross-tenant: handlers honor the tenant scope set during outbox dispatch.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated.
- [ ] Templates for each category exist (added in P6-T02 and updated here if needed).

## 8. Handoff notes / gotchas

- A child Player without `IsMinor=true` shouldn't auto-include parents in notifications (parents are CC'd only for minors). Document and test.
- Deactivated recipients are silently skipped, not errored.
- The handler runs inside the outbox dispatcher's tenant scope — do not assume `HttpContext`.

## 9. Suggested execution outline

1. `IRecipientResolver` + tests.
2. One handler at a time; tests per handler.
3. Validate end-to-end (booking → email visible in Mailpit + in-app).
4. CHANGELOG.

## 10. Open questions / risks

- Risk: notification storms when an admin cancels a big series. **Mitigation**: P6-T01's dispatcher already retries per delivery; SMTP provider rate limits will queue naturally. V2 may add per-tenant rate limiting.
