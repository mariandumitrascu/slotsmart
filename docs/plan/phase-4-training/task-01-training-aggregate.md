# Task `P4-T01` — Training Aggregate (One-Off)

> **Phase**: 4 — Training Management
> **Estimated size**: M
> **Depends on**: Phase 3 complete
> **Can run in parallel with**: nothing

---

## 1. Context

The `Training` aggregate is the heart of Phase 4 and Phase 5. We build the one-off (single-occurrence) version first so the surface is small and well-tested before recurring series add complexity.

## 2. Goal

> A Head Coach / Club Admin can create a one-off training; coaches can see their own trainings; everyone in the club can list visible (published) trainings; cancellation works.

## 3. Scope

### In scope

- Aggregate `Training` in `Domain/Trainings/`:
  - `Id`, `TenantId`, `Title`, `Description?`, `Kind` (`Group | Individual`), `StartUtc`, `EndUtc` (derived from `StartUtc + Duration` is fine too; pick one and stick to it — recommend storing both for query speed), `Location` (free text), `Capacity` (int ≥ 1, or 0 = uncapped behind a feature flag — see notes), `CoachIds` (1..3, validated as Coach role members), `Status` (`Draft | Published | Cancelled | Completed`), `SeriesId?` (null in this task), `Version`.
  - Invariants:
    - `StartUtc < EndUtc`, duration ≥ 15 min, ≤ 8 h.
    - At least 1 coach; max 3.
    - Capacity ≥ 1 (uncapped requires `AllowUncapped=true` flag on `ClubSettings`, default off).
    - `Kind=Individual` ⇒ Capacity == 1.
- Use cases:
  - `CreateTraining` (HeadCoach+).
  - `PublishTraining` (HeadCoach+).
  - `UpdateTraining` (HeadCoach+ for status=Draft|Published; cannot edit Cancelled/Completed).
  - `CancelTraining` (HeadCoach+; sets Cancelled; will be the trigger for refunds/notifications in later phases).
  - `ListTrainings` queries:
    - For Coach (`mine=true`): only sessions where they are a coach.
    - For Player/Parent: only Published + future.
    - For HeadCoach+: all statuses including Draft.
  - `GetTraining` by id (filtered by tenant + visibility).
- Endpoints:
  - `POST /api/v1/trainings`, `GET /api/v1/trainings`, `GET /api/v1/trainings/{id}`, `PATCH /api/v1/trainings/{id}`, `POST /api/v1/trainings/{id}:publish`, `POST /api/v1/trainings/{id}:cancel`.
- Audit: `training.created`, `training.published`, `training.updated`, `training.cancelled`.

### Out of scope

- Recurring (`TrainingSeries`) → **P4-T02**.
- Coach overlap detection → **P4-T04** (referenced as a TODO here).
- Attendance → **P4-T05**.
- Booking → **Phase 5**.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
- Env vars: none new.

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Trainings/Training.cs`, `TrainingKind.cs`, `TrainingStatus.cs`.
- EF config + migration `AddTrainings`.
- Application use cases + endpoints.
- Tests: invariants, list filtering by role, status transitions.
- Audit entries.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] HeadCoach creates a draft training; `GET /api/v1/trainings` as a Player does **not** show it.
- [ ] After publish, the Player sees it.
- [ ] Editing a Cancelled training returns `409`.
- [ ] Cancelling already-Cancelled training returns `409`.
- [ ] `Kind=Individual` with Capacity > 1 → `400`.
- [ ] Filter `?coachId=...&from=...&to=...&status=Published` works.
- [ ] Cross-tenant: a HeadCoach in tenant A cannot see tenant B's trainings.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- `CoachIds` is a value-collection on the aggregate; consider an owned entity `TrainingCoach` (so you can later add per-coach role like "Lead Coach" without breaking).
- Times are UTC in DB; UI converts to club timezone. Add a `ToClubLocal()` extension for display use.
- Validate that all `CoachIds` are Members in this tenant with `Coach` or `HeadCoach` role and `IsActive=true`.
- `Capacity` constraint at DB level: `CHECK (Capacity >= 1 OR Capacity = 0 AND AllowUncapped)` — actually keep it simple: `Capacity >= 1` and special-case uncapped later.

## 9. Suggested execution outline

1. Aggregate + invariants + tests (Domain only).
2. Migration.
3. Use cases + validators + endpoints.
4. Integration tests for filtering + visibility.
5. CHANGELOG.

## 10. Open questions / risks

- Question: separate "court" entity? **Decision (MVP)**: no — free text. Court reservation comes in V2.
