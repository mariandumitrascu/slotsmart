# Task `P4-T06` — Frontend: Training Calendar + Create/Edit

> **Phase**: 4 — Training Management
> **Estimated size**: L
> **Depends on**: P4-T01, P4-T02, P4-T03
> **Can run in parallel with**: P4-T07 (parts)

---

## 1. Context

The most prominent visual surface of the product: a weekly calendar of trainings. Coaches and admins create/edit from here.

## 2. Goal

> A signed-in member sees a weekly calendar of trainings (filtered by role). A HeadCoach can click an empty slot to create a one-off training or a series. Editing a training is a context-aware flow that offers the three scopes from P4-T03.

## 3. Scope

### In scope

- `features/training/` feature folder.
- Pages:
  - `pages/CalendarPage.tsx` — week view (Mon–Sun); month view as optional toggle. Use **FullCalendar** or **@mui/x-date-pickers**'s DateCalendar + a custom week renderer; recommend **FullCalendar** (mature, free react adapter).
  - `pages/TrainingDetailPage.tsx` — full info; edit / cancel / publish; attendees placeholder (P4-T07).
  - `pages/TrainingSeriesPage.tsx` — series detail with all occurrences, pause/end actions.
- Components:
  - `components/TrainingCreateDialog.tsx` — tabs: "Single" / "Recurring".
  - `components/EditScopeDialog.tsx` — prompts `this | thisAndFollowing | series` when editing/cancelling a recurring training.
  - `components/TrainingForm.tsx` — fields (title, coaches multi-select, kind, capacity, location, time).
- API hooks `api/trainings.api.ts`, `api/training-series.api.ts`.
- Role-aware UI:
  - Player: read-only calendar; clicking a training opens detail with a "Book" stub.
  - Coach: same + "Complete" + "Attendance" buttons on their trainings (the Attendance page is delivered in P4-T07).
  - HeadCoach / ClubAdmin: full CRUD.

### Out of scope

- Booking UI → Phase 5.
- Attendance UI → P4-T07.

## 4. Inputs

- API contracts from P4-T01..T03.
- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)

## 5. Deliverables

- All listed files.
- E2E test `frontend/tests/calendar.spec.ts`: HeadCoach creates a series; calendar shows all occurrences; edits one with scope `this`; verifies detached badge.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Calendar shows current week with trainings as colored blocks (color per kind or coach).
- [ ] Clicking a block opens detail. Editing presents scope dialog only for series occurrences.
- [ ] Create one-off training works (validation matches backend).
- [ ] Create recurring training works; the calendar reflects all occurrences in the visible weeks.
- [ ] Cancelling a series offers scope choice.
- [ ] Player view is read-only.
- [ ] All times displayed in club timezone (visible somewhere in the UI).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] All strings i18n'd.
- [ ] No `any`.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Server returns UTC times; convert to club timezone (read from `GET /api/v1/club`) for display.
- The coach multi-select must filter to active Coach/HeadCoach members; cache that list with TanStack Query.
- Form must show a clear error when backend returns `coach-overlap` 422, with a link to the conflicting training.

## 9. Suggested execution outline

1. Install FullCalendar + React adapter; render placeholder events.
2. Wire `trainings.api` + week-range filter.
3. Build create dialogs (single + series tabs).
4. Build edit + scope-aware dialog.
5. Build series detail page.
6. E2E test.
7. CHANGELOG.

## 10. Open questions / risks

- Risk: FullCalendar bundle size. **Mitigation**: use only the React + DayGrid/TimeGrid plugins; defer Calendar imports via lazy route.
