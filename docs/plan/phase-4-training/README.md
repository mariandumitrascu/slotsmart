# Phase 4 — Training Management

> Release: **1.0 (MVP)** · Depends on: Phase 3 complete · Unlocks: Booking (Phase 5).

## Goal

Give the club the ability to **schedule** trainings: one-off and recurring, with assigned coaches, capacity, location, and attendance tracking.

## Outcomes

- A Head Coach / Club Admin can create one-off trainings.
- They can create recurring trainings (a `TrainingSeries`) that materialize into N concrete `Training` instances.
- Each training has a coach (or coaches), a capacity, a start/end, and a location/court (free-text in MVP).
- Coaches see "my trainings" and can edit / cancel individual occurrences or the whole series.
- Attendance is recorded per training (states from the glossary).
- Coach-overlap conflicts are detected and rejected.
- Frontend has calendar + list views for trainings.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P4-T01 | [Training aggregate (one-off)](./task-01-training-aggregate.md) | M | P3 done |
| P4-T02 | [TrainingSeries + materialization](./task-02-training-series.md) | L | P4-T01 |
| P4-T03 | [Edit/cancel single occurrence vs series](./task-03-edit-cancel-rules.md) | M | P4-T02 |
| P4-T04 | [Coach overlap detection](./task-04-coach-overlap.md) | M | P4-T01 |
| P4-T05 | [Attendance tracking](./task-05-attendance.md) | M | P4-T01 |
| P4-T06 | [Frontend: training calendar + create/edit](./task-06-frontend-calendar.md) | L | P4-T01, P4-T02, P4-T03 |
| P4-T07 | [Frontend: attendance UI](./task-07-frontend-attendance.md) | M | P4-T05, P4-T06 |

## Acceptance criteria for the whole phase

- [ ] Head Coach creates "Monday & Wednesday 18:00–19:30, U12 group, Coach X, capacity 8" for 12 weeks → 24 `Training` rows are visible in the calendar.
- [ ] Editing a single occurrence (move time / change coach) does not affect the others.
- [ ] Editing the series ("this and all future") updates the right occurrences.
- [ ] Creating a training that puts a coach in two overlapping sessions returns `422` with the documented error.
- [ ] Coach can mark Present / Absent / Excused / Late for each expected player on a given training; states persist and audit.
- [ ] Frontend calendar shows trainings in week view; click → detail; Head Coach can create a new training from the calendar.
