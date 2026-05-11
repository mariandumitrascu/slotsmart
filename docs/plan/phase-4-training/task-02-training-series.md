# Task `P4-T02` — TrainingSeries + Materialization

> **Phase**: 4 — Training Management
> **Estimated size**: L
> **Depends on**: P4-T01
> **Can run in parallel with**: P4-T04, P4-T05

---

## 1. Context

Most trainings recur weekly. We add a `TrainingSeries` definition and a **materializer** that creates concrete `Training` rows for a configurable horizon ahead. Materialization is idempotent and runs both on demand (when creating/editing a series) and on a background schedule (rolling horizon).

## 2. Goal

> Creating a `TrainingSeries` like "Mondays & Wednesdays 18:00–19:30, U12 group, Coach X, capacity 8, until 2026-12-15" creates the series row and materializes one `Training` per scheduled occurrence within the configured horizon; daily background job extends the horizon.

## 3. Scope

### In scope

- Aggregate `TrainingSeries` in `Domain/Trainings/`:
  - `Id`, `TenantId`, `Title`, `Description?`, `Kind`, `Capacity`, `CoachIds`, `Location`, `DurationMinutes`.
  - Recurrence: `DaysOfWeek` (bitmask or set), `LocalTimeOfDay`, `TimeZoneId` (= club default unless overridden), `StartDate`, `EndDate?`.
  - `MaterializationHorizonDays` (default 56 = 8 weeks).
  - `Status` (`Active | Paused | Ended`).
  - `Version`.
  - Invariants: at least one weekday selected; horizon ≥ 7; same duration constraints as single training.
- Use cases:
  - `CreateTrainingSeries` (HeadCoach+) — creates the series and synchronously materializes the first horizon.
  - `PauseTrainingSeries`, `ResumeTrainingSeries`, `EndTrainingSeries`.
  - `UpdateTrainingSeries` (defaults: edits propagate to **future, untouched** occurrences only; see T03 for the rules engine).
- Materialization service `TrainingMaterializer`:
  - Given a series + a target date range, computes the set of expected occurrences and INSERTs the ones that don't exist (matching on `SeriesId + StartUtc`).
  - Idempotent — re-runs are safe.
  - Skips dates in tenant holiday list (V2 — for now, assume none).
- Background job (Quartz):
  - Runs daily at 02:00 club-time per active series — actually, run a single platform-wide daily job that iterates active series across all tenants in batches (use the tenant scope helper from P2-T01).
  - Extends each series's materialized horizon to `today + MaterializationHorizonDays`.
- Endpoints:
  - `POST /api/v1/training-series`
  - `GET /api/v1/training-series`
  - `GET /api/v1/training-series/{id}`
  - `PATCH /api/v1/training-series/{id}`
  - `POST /api/v1/training-series/{id}:pause`, `:resume`, `:end`
- The `Training` aggregate gains `SeriesId` (already declared in P4-T01).
- Audit entries.

### Out of scope

- Single-occurrence edits within a series ("this only" / "this and future") → **P4-T03**.
- Coach overlap → **P4-T04**.
- Holidays / exception dates → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)
- Env vars:
  - `Trainings__DefaultMaterializationHorizonDays=56`

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Trainings/TrainingSeries.cs`, `RecurrenceRule.cs` (value object).
- EF config + migration `AddTrainingSeries`.
- `backend/src/SlotSmart.Application/Features/TrainingSeries/...`
- `backend/src/SlotSmart.Application/Common/Services/TrainingMaterializer.cs`
- `backend/src/SlotSmart.Infrastructure/Jobs/TrainingMaterializationJob.cs` (Quartz).
- Quartz registration in `Infrastructure/DependencyInjection.cs`.
- Endpoints `TrainingSeriesEndpoints.cs`.
- Tests: materialization idempotency, time zone DST correctness, pause/resume semantics, performance test (a year of weekly series — must materialize in < 1s).
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Creating a series with Monday + Wednesday recurrence, 8-week horizon, materializes exactly 16 `Training` rows.
- [ ] Re-running materialization adds no duplicates.
- [ ] DST transitions: a Monday 18:00 local series produces a `Training` at the **correct local 18:00** on both sides of the transition (verified by a test using `Europe/Bucharest`).
- [ ] Pausing a series: future occurrences that haven't started yet are removed; past/in-progress occurrences are kept.
- [ ] Ending a series sets `EndDate=today`; future occurrences removed; series read endpoints still work.
- [ ] Background job extends the horizon: with `Trainings__DefaultMaterializationHorizonDays=56` and a series running for 1 year, after the job runs on day 7, there are 8 weeks of materialized occurrences ahead.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Time zones are the trickiest part. Store the recurrence in local time + a zone id; compute UTC at materialization time using `TimeZoneInfo.ConvertTimeToUtc`. **Never** assume "1 week = 7 × 24h" — that breaks DST.
- "Future occurrences that haven't started yet" means `StartUtc > now`. Past/in-progress instances retain their bookings (Phase 5).
- The background job must scope itself per tenant (`tenantScope.Begin(tenantId)`) when calling tenant-aware code.
- Add a unique index `(SeriesId, StartUtc)` on `Trainings` to make idempotency cheap.

## 9. Suggested execution outline

1. Aggregate + recurrence value object + tests (Domain only, including DST tests).
2. Migration.
3. `TrainingMaterializer` + tests.
4. Use cases + validators.
5. Endpoints.
6. Quartz job + tests (use the Quartz in-memory scheduler in tests).
7. CHANGELOG.

## 10. Open questions / risks

- Risk: materializing too far ahead bloats the table; too short and bookings open too late. **Decision**: default 56 days, configurable per series.
- Risk: editing a series in flight is conceptually tricky. **Mitigation**: that logic is fully delegated to P4-T03; this task only does CRUD + materialization.
