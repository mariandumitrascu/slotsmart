# Task `P3-T04` — Coach Profile Extension

> **Phase**: 3 — Club & Member Management
> **Estimated size**: M
> **Depends on**: P3-T02
> **Can run in parallel with**: P3-T03, P3-T05

---

## 1. Context

Coaches have richer profiles than other members: bio, specialization, photo, certifications. We model this as a `CoachProfile` extension entity (1:1 with `Member`) rather than fattening the `Member` aggregate, because it's optional and grows over time.

## 2. Goal

> A coach has a public-within-club profile editable by themselves or a Club Admin; players and parents can view it to choose trainings.

## 3. Scope

### In scope

- Entity `CoachProfile` in `Domain/Members/`:
  - `Id` (= `MemberId`), `TenantId`, `Bio` (max 2000 chars), `Specializations` (list of free-text tags, max 20), `PhotoUrl?`, `YearsOfExperience?`, `LanguagesSpoken` (BCP-47 list), `Version`, `UpdatedAt`.
- One-to-one with `Member`. `CoachProfile` only exists when the Member has the `Coach` role (or `HeadCoach`).
- Auto-creation: when a Member is given the `Coach` role for the first time, an empty `CoachProfile` is provisioned.
- Auto-archive: when the `Coach` role is removed, the profile is **soft-archived** (kept for historical bookings/training references) and not returned in default queries.
- Endpoints:
  - `GET /api/v1/coaches` — public listing within tenant (any authenticated member). Returns name + photo + specializations + bio.
  - `GET /api/v1/coaches/{memberId}` — same shape.
  - `PATCH /api/v1/coaches/{memberId}` — self or Club Admin.
- Audit `coach.profile.updated`.

### Out of scope

- Coach availability (working calendar) → **Phase 4** (used by Training scheduling).
- Coach ratings / reviews → V2.
- Coach payouts / fees → V2 (Payments).

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Members/CoachProfile.cs`
- EF config + migration `AddCoachProfiles`.
- Application use cases + endpoints `CoachesEndpoints.cs`.
- Hook into role change handler from P3-T03 (provision on add, archive on remove).
- Tests.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Adding the `Coach` role to a member creates an empty `CoachProfile` in the same transaction.
- [ ] Removing the `Coach` role archives the profile; subsequent `GET /api/v1/coaches` excludes it; `GET /api/v1/coaches/{id}` returns 404 unless `?includeArchived=true` and caller is Club Admin.
- [ ] A coach can patch their own profile; players/parents cannot.
- [ ] Validation: `Bio` ≤ 2000 chars, `Specializations` ≤ 20 items, each ≤ 32 chars, alphanumeric + space + dash.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Storing `Specializations` and `LanguagesSpoken` as `text[]` (Postgres array) is idiomatic; Npgsql + EF Core supports it natively.
- Treat the role-change handler as the only place that creates/archives `CoachProfile` — never expose a direct "create profile" endpoint.

## 9. Suggested execution outline

1. Entity + value rules.
2. Migration.
3. Hook into `UpdateMemberRoles` handler.
4. Application use cases + endpoints.
5. Tests + audit.

## 10. Open questions / risks

- Question: photo upload? **Decision (MVP)**: URL string only; blob storage in V2.
