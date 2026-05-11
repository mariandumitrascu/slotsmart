# Task `P3-T01` — Club Settings Domain & Endpoints

> **Phase**: 3 — Club & Member Management
> **Estimated size**: M
> **Depends on**: P2 done
> **Can run in parallel with**: P3-T02 (drafted)

---

## 1. Context

`Tenant` from Phase 2 holds only the bare minimum (id, slug, name, status, timezone, version). We need richer **club settings** so the rest of the product has somewhere to read defaults from: working hours, default booking lead times, default cancellation windows, currency for V2, public profile.

We extend the `Tenant` aggregate (the "Club") with a `ClubSettings` value object rather than introducing a separate aggregate.

## 2. Goal

> A Club Admin can fetch and update club settings via `GET /api/v1/club` and `PATCH /api/v1/club`. Other domain code (Phase 4/5) reads defaults from these settings via a small read service.

## 3. Scope

### In scope

- Extend `Tenant` (renamed in code to `Club` if helpful — keep table name `Tenants` for stability) with:
  - `DisplayName`
  - `LogoUrl` (nullable)
  - `Address` (free-text in MVP)
  - `ContactEmail`, `ContactPhone`
  - `WorkingHours` per weekday (open/close times)
  - `DefaultBookingLeadTimeHours` (how early bookings open before training start, default 168 = 7 days)
  - `DefaultCancellationWindowHours` (default 24)
  - `WaitingListEnabledByDefault` (bool)
  - `AutoPromoteFromWaitingList` (bool)
  - `Currency` (ISO 4217, default `EUR`; unused in MVP)
- `IClubSettingsReader` abstraction in `Application` so other features can read settings without depending on the full aggregate.
- Endpoints:
  - `GET /api/v1/club` (`AnyMember`) — full settings + public info.
  - `PATCH /api/v1/club` (`ClubAdmin`) — partial update (merge patch).
- Validation: working hours coherent (open < close), positive lead/cancellation times.
- Audit entry on every change.

### Out of scope

- Courts / facilities → V2.
- Holidays / non-working days → V2 (use a simple calendar of exceptions if needed in Phase 4 for recurring trainings).
- Billing plan editing → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Tenants/Tenant.cs` extended; new value objects `ClubContact`, `WeeklyWorkingHours`, `DayWorkingHours`.
- Migration `ExtendTenantWithClubSettings`.
- `backend/src/SlotSmart.Application/Features/Club/Queries/GetClub/...`
- `backend/src/SlotSmart.Application/Features/Club/Commands/UpdateClub/...`
- `backend/src/SlotSmart.Application/Common/Abstractions/IClubSettingsReader.cs` + EF impl in Infrastructure.
- `backend/src/SlotSmart.Api/Endpoints/ClubEndpoints.cs`
- Tests + audit assertion.
- CHANGELOG entry.

## 6. Acceptance Criteria

- [ ] `GET /api/v1/club` returns the full settings for the caller's tenant.
- [ ] `PATCH /api/v1/club` (Club Admin) updates only the fields provided; other fields untouched.
- [ ] Validation rejects incoherent working hours, negative lead times, etc.
- [ ] Non-admin members cannot PATCH (`403`).
- [ ] Audit entry `club.settings.changed` is recorded with a diff in `metadata`.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Working hours stored as a JSON column (jsonb) on the tenant row to avoid a new table. The value object is immutable.
- `Currency` is present today so we don't have to migrate everyone later.
- `LogoUrl` is just a string for MVP. File upload UI is a follow-up task in Phase 3 if needed; otherwise V2.

## 9. Suggested execution outline

1. Extend the `Tenant` aggregate with new fields + value objects + invariants.
2. Migration.
3. Application command/query + validator.
4. Endpoint group with merge-patch handling.
5. Tests + audit.
6. CHANGELOG.

## 10. Open questions / risks

- Question: store a logo as bytes or only URL? **Decision (MVP)**: URL only; uploads via a future blob storage task in V2.
