# Task `P3-T05` — Parent–Child Relations (MemberRelation)

> **Phase**: 3 — Club & Member Management
> **Estimated size**: M
> **Depends on**: P3-T02
> **Can run in parallel with**: P3-T03, P3-T04

---

## 1. Context

Children at a tennis club have a Parent who handles communication and booking on their behalf. We model this as a directed `MemberRelation` from Parent → Child, future-proofed for additional relation types (e.g. Guardian, Sibling).

## 2. Goal

> A Parent can be linked to N child Players; a child Player flagged as `IsMinor=true` cannot perform bookings or change their own profile — only their linked Parents (or a Club Admin) can.

## 3. Scope

### In scope

- Aggregate `MemberRelation` in `Domain/Members/`:
  - `Id`, `TenantId`, `FromMemberId` (the Parent), `ToMemberId` (the Child), `RelationType` (enum: `ParentOf` for MVP), `CreatedAt`, `Status` (`Active|Revoked`).
  - Uniqueness: a single `Active` relation per `(FromMemberId, ToMemberId, RelationType)`.
- Use cases:
  - `LinkParentToChild` (Club Admin): create relation.
  - `UnlinkParentFromChild` (Club Admin): revoke.
  - `ListRelationsForMember` (Coach+ for any; self for own outgoing relations).
- Authorization helper `IResourceAuthorizationService.CanActOnBehalfOf(Member actor, Member target)`:
  - True if actor == target.
  - True if actor has `ClubAdmin`.
  - True if there's an `Active` `ParentOf` relation from actor → target.
  - Otherwise false.
- `Member.IsMinor`:
  - Already derived in P3-T02 from `DateOfBirth`.
  - Add a domain rule: if `IsMinor=true`, a member with role `Player` cannot self-update profile or perform bookings (enforced via `IResourceAuthorizationService`).
- Endpoint set:
  - `POST /api/v1/members/{memberId}/relations` — body `{ relatedMemberId, relationType: "ParentOf" }`.
  - `DELETE /api/v1/members/{memberId}/relations/{relationId}`.
  - `GET /api/v1/members/{memberId}/relations` — both directions.
- Audit entries: `member.relation.linked`, `member.relation.unlinked`.

### Out of scope

- Self-service "claim my child" flow (parent invites a child) → V2.
- Multiple parents per child UX → endpoints support N relations; UX is in P3-T07.
- Sharing notifications across parent + child → Phase 6.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Members/MemberRelation.cs`, `RelationType.cs`.
- EF config + migration `AddMemberRelations`.
- `IResourceAuthorizationService` real impl (replaces P2-T03 stub).
- Endpoints + tests.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Linking a Parent to a Child creates one `Active` relation; duplicates return `409`.
- [ ] A non-minor Child cannot be auto-restricted (the IsMinor flag dictates restrictions).
- [ ] A Parent calling `GET /api/v1/members/{childId}` sees the child's full profile (PII allowed because of parent relation).
- [ ] A Parent calling `PATCH /api/v1/members/{childId}` updates the child's profile when `IsMinor=true`.
- [ ] An unrelated Player calling those same endpoints sees only public fields / gets 403 on writes.
- [ ] Audit entries land for every link/unlink.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- The `IResourceAuthorizationService` is now used in P3-T02's PATCH and `GET /members/{id}` flows. Re-test those paths after this task lands.
- For booking endpoints in Phase 5 we'll re-use `CanActOnBehalfOf` — keep its contract small and stable.

## 9. Suggested execution outline

1. Aggregate + invariants + tests.
2. Migration.
3. `IResourceAuthorizationService` real impl + unit tests.
4. Endpoints + integration tests.
5. Update P3-T02's member endpoints to use the service.
6. CHANGELOG.

## 10. Open questions / risks

- Question: do we let multiple Parents be linked to one Child? **Decision**: yes, N is allowed.
- Risk: cycles. **Mitigation**: validate `FromMemberId != ToMemberId`; cycles longer than 1 are uninteresting for `ParentOf`. Future relation types may need a cycle check.
