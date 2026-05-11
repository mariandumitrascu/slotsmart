# Task `P4-T04` — Coach Overlap Detection

> **Phase**: 4 — Training Management
> **Estimated size**: M
> **Depends on**: P4-T01
> **Can run in parallel with**: P4-T02, P4-T03, P4-T05

---

## 1. Context

A coach cannot be in two places at once. We detect overlapping training assignments at create / update / materialize time and reject (or warn) clearly.

## 2. Goal

> Attempting to create or update a training (or series) that would put a coach into two overlapping sessions returns `422` with the conflicting training(s) in the response body.

## 3. Scope

### In scope

- A domain service `ICoachConflictDetector` in `Application/Features/Trainings/Services/`:
  - `Task<ConflictReport> DetectAsync(IEnumerable<Guid> coachIds, DateTimeOffset startUtc, DateTimeOffset endUtc, Guid? excludeTrainingId, Guid? excludeSeriesId, CancellationToken ct)`.
  - Implementation queries the DB with proper indexes.
- Integration into:
  - `CreateTraining` — reject conflicts.
  - `UpdateTraining` — reject conflicts (excluding the training itself).
  - `CreateTrainingSeries` materialization — if any occurrence would conflict, reject **the entire series** by default; allow `?onConflict=skip` to skip conflicting occurrences and report them.
  - `UpdateTrainingSeries` (scope `series` or `thisAndFollowing`) — same.
- Response shape for conflicts: `422` + problem+json with `type=slotsmart/errors/coach-overlap` and `errors.conflicts` listing the offending training summaries.

### Out of scope

- Coach calendar / availability windows ("Coach X only works Tue/Thu") → V2.
- Configurable "warn vs reject" per club → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
- DB index considerations.

## 5. Deliverables

- `ICoachConflictDetector` + EF Core implementation.
- Migration adding indexes:
  - On `TrainingCoaches(CoachId, StartUtc, EndUtc)` (or wherever the join lives).
  - Composite tenant + time indexes for fast windowed lookups.
- Integration with the create/update commands.
- Tests covering:
  - Direct overlap.
  - Touch-edge (back-to-back) — **not** an overlap.
  - Cross-day overlap.
  - Update that moves a session OUT of conflict.
  - Materialization with `onConflict=reject` and `onConflict=skip`.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Two trainings 18:00–19:00 and 18:30–19:30 with the same coach can't both exist; the second one returns `422` with the first in `errors.conflicts`.
- [ ] Two trainings 18:00–19:00 and 19:00–20:00 (back-to-back) are allowed.
- [ ] Materializing a Monday series with a coach who already has a one-off on the first Monday: with `onConflict=reject` the call fails; with `onConflict=skip` the series is created and the response includes `skippedOccurrences`.
- [ ] Indexes exist and `EXPLAIN ANALYZE` of the conflict query uses them (verified manually; no test required).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI examples updated for the new error.
- [ ] Client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- The overlap query is the classic interval-overlap: `existing.StartUtc < new.EndUtc AND existing.EndUtc > new.StartUtc`.
- Be sure to exclude `Status=Cancelled` trainings from the conflict set.
- For series materialization, batch the conflict checks: one query per series with a window covering the full horizon, not N queries.

## 9. Suggested execution outline

1. Service interface + EF impl + tests.
2. Wire into create/update commands.
3. Wire into materializer.
4. Migration with indexes.
5. CHANGELOG.

## 10. Open questions / risks

- Question: warn vs reject when a club admin explicitly forces a double-booking? **Decision (MVP)**: reject; allow `force=true` only via Club Admin and audit it loudly.
