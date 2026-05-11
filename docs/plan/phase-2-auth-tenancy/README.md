# Phase 2 — Authentication & Multi-Tenancy

> Release: **1.0 (MVP)** · Depends on: Phase 1 complete · Unlocks: every feature phase.

## Goal

Turn the foundation into a **secure, tenant-aware** application. After this phase, any feature work in Phase 3+ can simply declare "I need an authenticated user with role X in tenant Y" and the platform takes care of the rest.

## Outcomes

- A user can sign up a **new club** (tenant + first admin user) via a public flow.
- A user can sign in and receive an access token + refresh token.
- All `/api/v1/*` endpoints (except a small allow-list) require authentication.
- Multi-tenant data isolation is enforced via EF Core query filters + write-side stamping.
- Role-based access control works end-to-end: roles are claims; policies are declared per endpoint.
- The frontend has an auth flow: sign-up → sign-in → routed to a placeholder authenticated home page; a 401 triggers a refresh + retry; logout works.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P2-T01 | [Tenant resolution & EF query filters](./task-01-tenant-resolution.md) | M | P1 done |
| P2-T02 | [Authentication: OpenIddict + JWT + refresh tokens](./task-02-auth-openiddict.md) | L | P2-T01 |
| P2-T03 | [Role-based access control (RBAC) end-to-end](./task-03-rbac.md) | M | P2-T02 |
| P2-T04 | [Club signup & first admin onboarding](./task-04-club-signup.md) | M | P2-T02, P2-T03 |
| P2-T05 | [Frontend auth flow (sign up, sign in, logout, refresh)](./task-05-frontend-auth.md) | L | P2-T02 |
| P2-T06 | [Audit log infrastructure](./task-06-audit-log.md) | S | P2-T02 |

P2-T01 first. P2-T02/T03/T04/T06 are largely sequential. T05 can begin once T02 has a stable token endpoint.

## Acceptance criteria for the whole phase

- [ ] Anonymous user can hit `POST /api/v1/auth/clubs:signup` with `{ clubName, slug, adminEmail, adminPassword, adminName }` and create a tenant + first user with `ClubAdmin` role.
- [ ] Anonymous user can hit `POST /api/v1/auth/token` (password grant) and receive an access + refresh token.
- [ ] Authenticated user can hit `GET /api/v1/me` and get their profile + roles + tenant.
- [ ] All endpoints require authentication except the documented allow-list (signup, token, refresh, health, openapi, docs).
- [ ] Attempting to read data from another tenant returns 404 (does not leak existence).
- [ ] Frontend can sign up, sign in, refresh, and log out; the protected `/app` route group requires auth.
- [ ] Audit log records `MemberCreated`, `MemberRoleChanged`, `SignInSucceeded`, `SignInFailed`.
- [ ] All endpoints have RBAC policies attached; an architecture test fails the build if a controller/endpoint is missing an `[Authorize(policy=...)]` (or explicit `[AllowAnonymous]`).
