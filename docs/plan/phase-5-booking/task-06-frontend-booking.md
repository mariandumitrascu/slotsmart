# Task `P5-T06` — Frontend: Book/Cancel on Calendar + "My Bookings"

> **Phase**: 5 — Booking System
> **Estimated size**: L
> **Depends on**: P5-T01, P5-T02, P5-T03, P5-T04
> **Can run in parallel with**: nothing significant

---

## 1. Context

The booking experience must feel instant and forgiving (clear errors, optimistic UI, easy cancellation).

## 2. Goal

> A Player or Parent can book a training in one click, see the result (Confirmed / WaitingList), cancel it in one click, and manage everything from a dedicated "My bookings" page.

## 3. Scope

### In scope

- `features/booking/`:
  - `components/BookButton.tsx` — handles Confirmed / WaitingList / disabled states based on training availability + window.
  - `components/BookOnBehalfPicker.tsx` — for Parents: picker of linked children + self (Parent-as-Player only if Parent has the Player role).
  - `components/CancelBookingDialog.tsx` — shows `lateCancel` warning if applicable.
  - `pages/MyBookingsPage.tsx` — tabs: Upcoming / Past; per-row Cancel.
  - `api/bookings.api.ts`.
- Calendar (P4-T06) integration: each training tile shows a Book button when applicable; when the user already has a booking, the tile shows the status badge.
- Idempotency: client generates an `Idempotency-Key` (uuid) per booking attempt and reuses it on retry.
- Error mapping for problem+json codes:
  - `training-full` → message + "Join waiting list" CTA (if waitlist disabled, just message).
  - `booking-not-yet-open` / `booking-closed` → human time of open/close.
  - `coach-overlap` / `409` duplicates → friendly messages.
- Empty states for My Bookings (Upcoming / Past).

### Out of scope

- Notifications UI → Phase 6.

## 4. Inputs

- API contracts from P5-T01..T05.
- Generated client.

## 5. Deliverables

- All listed files.
- E2E test `frontend/tests/booking.spec.ts`:
  - Player books a published training → Confirmed.
  - Same player tries again → friendly "already booked" message.
  - Player cancels → tile shows "Book" again.
  - Parent books on behalf of child → Confirmed; child's My Bookings reflects it (visible to Parent via "My family").
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Booking from the calendar updates the tile within ~150ms (optimistic), with rollback on failure.
- [ ] My Bookings shows upcoming and past, sorted newest-first for past and earliest-first for upcoming.
- [ ] Late cancellation: the dialog warns clearly; cancellation still succeeds.
- [ ] Parents see a "Book on behalf of" picker on the calendar that includes self and linked children; default is the actor.
- [ ] All error states render readable messages tied to the problem+json `type`.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] All strings i18n'd.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Always pass `Idempotency-Key`. The fetch wrapper can attach it automatically when the caller doesn't supply one — but make sure retries within the SAME user gesture reuse the same key.
- A child without an explicit "DateOfBirth" cannot be marked minor; if a Parent picks such a child, the button still works (no minor restrictions apply).
- Time zone display: again, club timezone is the source of truth in UI.

## 9. Suggested execution outline

1. `bookings.api.ts` hooks + error mapper.
2. BookButton + integration in calendar tile.
3. Cancel dialog + late-cancel warning.
4. My Bookings page.
5. Parent on-behalf picker (uses relations API).
6. E2E test.
7. CHANGELOG.

## 10. Open questions / risks

- Risk: race between calendar refresh and a just-confirmed booking. **Mitigation**: invalidate the trainings list query on booking success.
