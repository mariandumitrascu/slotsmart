# Task `P4-T07` — Frontend: Attendance UI

> **Phase**: 4 — Training Management
> **Estimated size**: M
> **Depends on**: P4-T05, P4-T06
> **Can run in parallel with**: nothing significant

---

## 1. Context

Coaches need a fast, tactile UI to mark attendance — typically on a phone, court-side. Optimize for speed and large tap targets.

## 2. Goal

> A coach opens a training and sees a list of expected players; one tap toggles between status states with clear visual feedback; the page works well on mobile.

## 3. Scope

### In scope

- `features/training/attendance/`:
  - `pages/AttendancePage.tsx` (`/app/trainings/:id/attendance`).
  - `components/AttendanceRow.tsx` — name, avatar, status segmented control (`Expected | Present | Late | Excused | Absent`), optional note (collapsible).
  - `components/AttendanceSummary.tsx` — counts of each status.
  - `components/CompleteTrainingButton.tsx` — confirmation, disables once Completed.
- API hooks `api/attendance.api.ts`.
- Optimistic mutations: tapping a status immediately changes the UI; on error, rollback + snackbar.
- Layout:
  - Sticky header with training info + summary.
  - Large tap targets (≥44px).
- Role-aware: only the training's coach (or HeadCoach+) can access; non-eligible users see `403` page.

### Out of scope

- Adding expected players from the UI (use API for now; the formal "add expected" flow is automated via bookings in Phase 5).

## 4. Inputs

- API contracts from P4-T05.

## 5. Deliverables

- All listed files.
- E2E test `frontend/tests/attendance.spec.ts`: Coach marks attendance; Complete the training; verify it's read-only.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Tapping a status updates the row instantly; a failure shows a snackbar and reverts.
- [ ] Summary updates live as marks change.
- [ ] After "Complete training", controls disable and a banner shows "Completed at <local time>".
- [ ] Mobile layout: rows comfortable to tap; status control is segmented and accessible (ARIA).
- [ ] Non-coaches see a 403 page.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] All strings i18n'd.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Use TanStack Query's `optimisticUpdate` pattern; keep the rollback logic in one place.
- Note input: keep a debounced PATCH so each keystroke isn't a request.

## 9. Suggested execution outline

1. Page + API hooks.
2. AttendanceRow component with optimistic updates.
3. Complete button.
4. E2E test.

## 10. Open questions / risks

- none significant.
