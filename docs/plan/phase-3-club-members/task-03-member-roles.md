# Task `P3-T03` — Role Management on Member

> **Phase**: 3 — Club & Member Management
> **Estimated size**: M
> **Depends on**: P3-T02
> **Can run in parallel with**: P3-T04, P3-T05

---

## 1. Context

A member's roles can change over time (a player becomes a coach; a coach is promoted to head coach). We give Club Admins a dedicated endpoint to manage roles, with clear audit and re-issuance semantics.

## 2. Goal

> A Club Admin can add/remove roles on any Member; the change is audited; the affected user's next sign-in (or refresh) emits a token with the updated roles. There is a documented but **non-mandatory** "force re-login" knob for security-sensitive demotions.

## 3. Scope

### In scope

- Endpoint `PUT /api/v1/members/{id}/roles` — body `{ roles: ["Coach", "Player"] }`. Validation: ≥1 role.
- Endpoint `POST /api/v1/members/{id}/roles:add` and `:remove` (sugar). Equivalent server-side.
- Authorization: `ClubAdmin` only.
- Invariant: a Member must always retain ≥1 role.
- Edge case: removing the last `ClubAdmin` role from the tenant is forbidden (must always have at least one active Club Admin). Returns `422` with `type=slotsmart/errors/last-club-admin`.
- Edge case: a member cannot change their own roles unless they are also Club Admin and there is at least one other Club Admin in the tenant.
- Re-issuance:
  - Tokens carry roles, so changes take effect on next access-token issue.
  - Add `Member.SecurityStamp` (or reuse the user's) that the token validator can compare against the DB on each request to opt into "live revocation" later. For MVP, **do not** do per-request DB lookups; document this as the trade-off.
  - Add a `POST /api/v1/members/{id}/sessions:revoke` (ClubAdmin) that revokes all refresh tokens for the user, forcing re-login.
- Audit `member.role.changed`.

### Out of scope

- Fine-grained permissions beyond role-level → V2.
- Cross-tenant role mapping → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Depends on `Role` enum and policies from P2-T03.

## 5. Deliverables

- `backend/src/SlotSmart.Application/Features/Members/Commands/UpdateMemberRoles/...`
- `backend/src/SlotSmart.Application/Features/Members/Commands/RevokeMemberSessions/...`
- Update `MembersEndpoints.cs`.
- Tests covering last-admin invariant, self-change rule, and audit.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] `PUT /api/v1/members/{id}/roles` updates roles for Coach+ targets and audits the change with `metadata.added` and `metadata.removed`.
- [ ] Removing all roles returns `400` ("at least one role required").
- [ ] Removing the only Club Admin returns `422` with the documented error type.
- [ ] After role change, the user's next refresh produces a token with the new roles claim.
- [ ] `POST /api/v1/members/{id}/sessions:revoke` invalidates outstanding refresh tokens (next refresh attempt fails with `invalid_grant`).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Be careful with concurrent role changes — use the aggregate's `Version` + `If-Match` for optimistic concurrency.
- Token contains roles at issue time; to make the change "feel" instant in the UI, the frontend should call `/auth/token/refresh` after a role change of the current user.

## 9. Suggested execution outline

1. Command/handler/validator + the last-admin invariant test.
2. Sessions:revoke command + integration with OpenIddict's token store.
3. Endpoints with policies.
4. Tests + audit.
5. CHANGELOG.

## 10. Open questions / risks

- Question: live token revocation (per-request DB check)? **Decision (MVP)**: no — accept the latency. Use sessions:revoke for security-sensitive demotions.
