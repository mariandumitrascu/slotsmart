# Task `P2-T02` — Authentication: OpenIddict + JWT + Refresh Tokens

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: L
> **Depends on**: P2-T01
> **Can run in parallel with**: nothing in Phase 2

---

## 1. Context

We host our own authorization server using **OpenIddict** so we can issue JWT access tokens + refresh tokens to our React app and later to mobile/other clients. We choose OpenIddict over plain ASP.NET Core Identity because we want a clean OAuth2 / OIDC surface that we can extend (PKCE, external IdPs) without rewriting.

## 2. Goal

> A valid sign-in returns an access token + refresh token; protected endpoints accept the bearer token; the refresh endpoint returns a new pair; tokens carry `sub`, `tenant_id`, and role claims; expired access tokens get a 401 that the frontend can refresh from.

## 3. Scope

### In scope

- Add OpenIddict to `SlotSmart.Infrastructure` + `SlotSmart.Api`.
- Define an **ApplicationUser** entity in `Domain` (or in `Infrastructure/Identity/` if we keep identity tables separate from domain): `Id (Guid)`, `Email`, `NormalizedEmail`, `EmailConfirmed`, `PasswordHash`, `SecurityStamp`, `TenantId`, `IsActive`, plus standard identity fields. Use **ASP.NET Core Identity** (`IdentityCore<ApplicationUser>`) only for password hashing + user store, paired with OpenIddict for token issuance.
- Configure OpenIddict to expose:
  - `POST /api/v1/auth/token` — `grant_type=password` (MVP) and `grant_type=refresh_token`.
  - Token format: **JWT**, signed with a development key in dev and a `KeyVault`-style key in prod (env-driven).
- Access token: short-lived (15 min). Refresh token: rolling, 14 days, single-use, server-tracked.
- JWT claims:
  - `sub` = `User.Id`
  - `tenant_id`
  - `email`
  - `roles` (multi-value)
  - `mid` = `Member.Id` (added once we have members; see P3)
- Configure ASP.NET Core JWT bearer to validate against the local issuer.
- Endpoint: `GET /api/v1/me` returns the current user + tenant (placeholder for member info until P3).
- Default authorization policy: every endpoint requires authentication except an explicit allow-list. Use `app.MapXxx().RequireAuthorization()` by default via `AuthorizationOptions.FallbackPolicy`.
- Add a development seed for: one tenant, one admin user, with deterministic credentials (configured via `appsettings.Development.json`).

### Out of scope

- Email confirmation / password reset flows → P2-T04 (signup) covers initial; reset is a small follow-up.
- External IdP (Google, Microsoft) → V2.
- MFA → V2.
- API keys / service-to-service auth → V2.
- Authorization code + PKCE flow for SPA → V2 (we'll start there but ROPC is simpler for MVP behind our own UI; document the trade-off).

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)
- Env vars introduced:
  - `Jwt__Issuer`
  - `Jwt__Audience`
  - `Jwt__SigningKey` (dev only; in prod use a certificate via Key Vault — out of scope here)
  - `Auth__AccessTokenLifetimeMinutes=15`
  - `Auth__RefreshTokenLifetimeDays=14`

## 5. Deliverables

### Domain / Identity

- `backend/src/SlotSmart.Infrastructure/Identity/ApplicationUser.cs`
- `backend/src/SlotSmart.Infrastructure/Identity/ApplicationRole.cs` (if we use Identity roles; otherwise keep roles on Member — see notes)
- DbContext extensions for Identity + OpenIddict tables.

### Infrastructure

- `backend/src/SlotSmart.Infrastructure/Identity/DependencyInjection.cs` — adds Identity Core, password options.
- `backend/src/SlotSmart.Infrastructure/Identity/OpenIddictConfig.cs` — adds OpenIddict core/server/validation, signing/encryption credentials, grant flows.
- `backend/src/SlotSmart.Infrastructure/Identity/TokenIssuer.cs` — central place to build claims for a user (tenant_id, roles, etc.).
- Migration `AddIdentityAndOpenIddict`.

### API

- `backend/src/SlotSmart.Api/Endpoints/AuthEndpoints.cs` — `/auth/token`, `/auth/token/refresh`, `/me`.
- Update `Program.cs`:
  - `AddAuthentication().AddJwtBearer(...)`
  - `AddAuthorization(o => o.FallbackPolicy = ...)`
  - Place `UseAuthentication()` and `UseAuthorization()` correctly relative to `TenantResolutionMiddleware`.

### Tests

- Endpoint tests: password grant happy path, wrong password (`400 invalid_grant`), refresh happy path, refresh re-use (single-use enforcement).
- A test that asserts the access token contains `tenant_id` and roles.
- A test that asserts an unauthenticated request to a non-allow-listed endpoint returns 401.

### Docs

- `CHANGELOG.md` updated.
- Add a short note to `docs/plan/00-architecture/api-conventions.md` if any decision needs to be captured (only if you change a convention).

## 6. Acceptance Criteria

- [ ] `POST /api/v1/auth/token` with `{ grant_type: "password", username, password, scope: "openid profile email offline_access" }` returns `{ access_token, refresh_token, expires_in, token_type: "Bearer" }`.
- [ ] The access token decodes to claims including `sub`, `tenant_id`, `email`, `roles`.
- [ ] `POST /api/v1/auth/token/refresh` exchanges a refresh token for a new pair; using the same refresh token a second time fails with `invalid_grant`.
- [ ] `GET /api/v1/me` returns the current user info and 401 when no bearer is sent.
- [ ] An expired access token returns 401 with `WWW-Authenticate: Bearer error="invalid_token"`.
- [ ] An architecture test fails the build if any endpoint is missing an authorization policy AND doesn't carry `[AllowAnonymous]`.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated under `Added`.
- [ ] OpenAPI spec includes the `BearerAuth` security scheme and applies it by default.
- [ ] The frontend client is regenerated and committed.
- [ ] Dev seed creates `admin@dev.local` / `Dev!2345` deterministically for the local tenant.

## 8. Handoff notes / gotchas

- Where do **roles** live? In MVP, roles are on the `Member` aggregate (see P3) and also flattened into the access token at issue time. Identity's `Roles` table is **not** used; we only use Identity for `UserManager` / `PasswordHasher`. Document this clearly so contributors don't accidentally call `userManager.AddToRoleAsync`.
- ROPC (password grant) is **not** OAuth2 best practice for SPAs. We use it because we control both ends in MVP. **Document the migration path to Authorization Code + PKCE** in this task's PR description so V2 can pick it up.
- Set `IdentityOptions.Password.RequiredLength=10`, require digits + non-alphanumeric. Configurable per environment.
- Use **rolling refresh tokens** with **reuse detection**. If a refresh token is reused, revoke the whole chain and force re-login.
- Store refresh tokens server-side (OpenIddict does this via `Authorizations` + `Tokens` tables).
- Be sure to place `UseAuthentication()` **before** the `TenantResolutionMiddleware` so the middleware can read `User.FindFirst("tenant_id")`.

## 9. Suggested execution outline

1. Add Identity Core + OpenIddict + EF stores; create migration.
2. Configure OpenIddict server with password + refresh_token grants, JWT format, signing key from config.
3. Implement `AuthEndpoints` with proper claims principal building.
4. Add JWT bearer auth + fallback authorization policy.
5. Implement `TenantContext` JWT path so `tenant_id` from the token wins over header.
6. Add dev seed.
7. Tests.
8. Regenerate OpenAPI snapshot, regenerate frontend client, commit.
9. CHANGELOG + PR description with migration-to-PKCE notes.

## 10. Open questions / risks

- Question: do we want to store hashed refresh tokens or rely on OpenIddict's defaults? **Decision**: OpenIddict's defaults (encrypted token reference in DB) are fine for MVP.
- Risk: clock skew between issuer and validators if we deploy multi-instance. **Mitigation**: `JwtBearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(1)` and ensure all instances use NTP.
- Risk: a user changes tenant — there's no mechanism in MVP. **Decision**: not allowed; documented in `domain-glossary.md`.
