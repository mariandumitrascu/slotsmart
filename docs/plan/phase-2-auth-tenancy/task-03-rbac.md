# Task `P2-T03` — Role-Based Access Control (RBAC) End-to-End

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: M
> **Depends on**: P2-T02
> **Can run in parallel with**: P2-T06

---

## 1. Context

Five roles are described in the project: **Player**, **Parent**, **Coach**, **HeadCoach**, **ClubAdmin** (plus the cross-tenant **PlatformAdmin**). A user can hold multiple roles. We need a clean way to declare per-endpoint role requirements and to enforce them with policies that are uniform, testable, and discoverable.

## 2. Goal

> Every endpoint declares the role(s) it requires via a typed policy attribute; an architecture test fails the build if a non-`AllowAnonymous` endpoint has no policy attached; tests verify positive and negative paths for one endpoint per policy.

## 3. Scope

### In scope

- Define a `Role` enum in `Domain` with values `Player`, `Parent`, `Coach`, `HeadCoach`, `ClubAdmin`, `PlatformAdmin`. (Strings on the wire, but typed in code.)
- Define standard **policies** in `Application/Common/Authorization/Policies.cs`:
  - `Policies.PlatformAdmin`
  - `Policies.ClubAdmin`
  - `Policies.CoachOrAbove` (HeadCoach, ClubAdmin)
  - `Policies.HeadCoachOrAbove`
  - `Policies.AnyMember` (default for endpoints any authenticated tenant member can call)
- Implement an authorization handler that reads `roles` claims and matches the requirement.
- Provide a `RequireRolesAttribute(params Role[])` for endpoint definitions OR (preferred for Minimal APIs) a small `RequirePolicy(Policy)` extension and `RequireAnyRole(...)` helper. Document the chosen approach.
- Implement `IResourceAuthorizationService` for resource-scoped checks (e.g. "this Parent can act on behalf of THIS child"). Stubbed here, used in P3+.
- Architecture test: every endpoint is either `[AllowAnonymous]` or has at least one of: `[Authorize(Policy=...)]`, `.RequireAuthorization(...)`. The test enumerates the endpoint data source.
- Update `/me` endpoint to expose the user's effective roles.

### Out of scope

- Permission-level RBAC (fine-grained "can_edit_training") → V2.
- Owner-based resource checks → handled per-feature as policies are needed.
- UI hiding/disabling of features by role → frontend handles this (P2-T05 and beyond).

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Members/Role.cs` (enum)
- `backend/src/SlotSmart.Application/Common/Authorization/Policies.cs`
- `backend/src/SlotSmart.Application/Common/Authorization/HasAnyRoleRequirement.cs` + handler
- `backend/src/SlotSmart.Application/Common/Authorization/IResourceAuthorizationService.cs` + stub impl
- Update `TokenIssuer.cs` to put roles as multi-value `roles` claims.
- `Program.cs`: `AddAuthorization(o => { foreach policy ... })`.
- `backend/tests/SlotSmart.Architecture.Tests/EndpointAuthorizationTests.cs`
- `backend/tests/SlotSmart.Api.Tests/AuthorizationTests.cs` — for each policy, one positive and one negative test against a representative endpoint.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] `Policies.cs` is the single source of truth for policy names.
- [ ] A request to a `ClubAdmin`-only endpoint with a `Coach` token returns `403 Forbidden`.
- [ ] A request to a `CoachOrAbove` endpoint with a `Player` token returns `403`.
- [ ] A request to an `AnyMember` endpoint succeeds for any authenticated tenant member.
- [ ] The architecture test fails when an endpoint is added without a policy and without `[AllowAnonymous]`.
- [ ] `/api/v1/me` returns `roles: ["Coach", "Player"]` for a user with multiple roles.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI schema for `/me` includes roles.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- ASP.NET Core's default `[Authorize(Roles="...")]` uses the `ClaimTypes.Role` claim. Map your `roles` claim to it in JwtBearer options (`TokenValidationParameters.RoleClaimType = "roles"`).
- Prefer policies over `[Authorize(Roles=...)]` strings sprinkled in code — easier to refactor.
- For Minimal APIs, the architecture test should iterate `IEndpointRouteBuilder.DataSources` and inspect endpoint metadata; do not rely on reflection on controllers (there might not be any).
- Roles in MVP are **per-tenant** but represented as flat strings in claims. When we add cross-tenant memberships in V2, this becomes `roles: ["tenantA:Coach", "tenantB:Player"]` or similar; design the handler so this evolution is smooth.

## 9. Suggested execution outline

1. Add `Role` enum + `Policies` constants.
2. Add the policy registrations and the `HasAnyRoleRequirement` handler.
3. Update `TokenIssuer` to emit `roles` claim from the user's effective roles (stubbed source until P3).
4. Map `RoleClaimType`.
5. Add tests + architecture test.
6. Update OpenAPI examples + regenerate client.
7. CHANGELOG.

## 10. Open questions / risks

- Question: do we want a separate `roles` aggregate per tenant or just an enum? **Decision (MVP)**: enum; the aggregate concept appears in Member (P3) where a Member has a `IReadOnlyCollection<Role>`.
