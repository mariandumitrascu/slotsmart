# Task `P4-T03` — Edit / Cancel: Single Occurrence vs Series

> **Phase**: 4 — Training Management
> **Estimated size**: M
> **Depends on**: P4-T02
> **Can run in parallel with**: P4-T04, P4-T05

---

## 1. Context

Calendars need three editing modes: "this only", "this and following", "the whole series". We codify the rules here so the rest of the codebase doesn't reinvent them.

## 2. Goal

> A HeadCoach editing a training (e.g. moving it 30 minutes later) can choose `scope = this | thisAndFollowing | series`; the system applies the change consistently and re-materializes as needed.

## 3. Scope

### In scope

- `PATCH /api/v1/trainings/{id}` accepts `?scope=this|thisAndFollowing|series` query param. Default `this`.
- Behavior:
  - `this`:
    - Marks the `Training` as **detached** (`IsDetached=true`); subsequent series edits ignore it.
    - Updates the fields on that single row.
  - `thisAndFollowing`:
    - Splits the series: keeps the existing series for occurrences before `this.StartUtc`; creates a **new** series starting at `this.StartUtc` with the updated fields; re-materializes the future from the new series; existing future occurrences not yet detached are replaced.
    - Detached future occurrences are NOT touched.
  - `series`:
    - Updates the series in place.
    - Re-materializes the future occurrences; detached ones are NOT touched.
- `POST /api/v1/trainings/{id}:cancel` accepts the same `scope` param. `cancel` of `this` marks the single occurrence Cancelled; `series` ends the series; `thisAndFollowing` splits and ends the new tail.
- Audit: include the scope and the resulting affected counts in `metadata`.

### Out of scope

- Conflict resolution when a re-materialization would clobber bookings → in Phase 5 we'll add a guard; for this task, assume no bookings yet (since we're still in Phase 4).

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)

## 5. Deliverables

- `Training.IsDetached` flag added (migration `AddIsDetachedToTrainings`).
- `TrainingSeries` "split" use case in `Application/Features/TrainingSeries/Commands/SplitSeries/...`.
- Extend `UpdateTraining`, `CancelTraining` to handle scope.
- Tests covering each scope across detached and non-detached neighbors.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Editing `this` (e.g. moving 30 min later) detaches the row; later edits to the series do not change it back.
- [ ] Editing `thisAndFollowing`: the original series keeps past occurrences; a new series owns future ones with the new fields.
- [ ] Editing `series`: all non-detached future occurrences are updated; past occurrences untouched.
- [ ] Cancel with `scope=thisAndFollowing` ends the new tail; series listing shows it as `Ended`.
- [ ] Detached rows are visually marked in the response (boolean field `isDetached: true`).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- "Replace future" is destructive. Take a transactional approach: compute the new set, soft-delete the old future set, insert the new set. Outside Phase 5's bookings concern, no data is lost.
- Make sure `thisAndFollowing` doesn't create duplicate `(SeriesId, StartUtc)` rows when the new series is materialized — wipe before re-materialize.

## 9. Suggested execution outline

1. Add the flag + migration.
2. `SplitSeries` use case + tests.
3. Extend `UpdateTraining`, `CancelTraining` with scope branching.
4. Endpoint params + OpenAPI examples.
5. CHANGELOG.

## 10. Open questions / risks

- Risk: surprising UX if a user expects "this and following" to keep bookings on detached rows. **Mitigation**: clearly document and surface in UI in P4-T06.
