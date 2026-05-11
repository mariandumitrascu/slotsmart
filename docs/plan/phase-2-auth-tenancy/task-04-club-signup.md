# Task `P2-T04` — Club Signup & First Admin Onboarding

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: M
> **Depends on**: P2-T02, P2-T03
> **Can run in parallel with**: P2-T05 (frontend can stub before this is ready)

---

## 1. Context

A new tennis club must be able to onboard itself: pick a slug, create the tenant, and create the first admin user. This is the only "public, no tenant required" data-writing endpoint in the system.

## 2. Goal

> A successful `POST /api/v1/auth/clubs:signup` creates a `Tenant`, an `ApplicationUser` with `ClubAdmin` role, and returns a token pair so the new admin can immediately use the app.

## 3. Scope

### In scope

- Endpoint `POST /api/v1/auth/clubs:signup` — `[AllowAnonymous]`, `[AllowAnonymousTenant]`.
- Request body:
  ```json
  {
    "club": { "name": "string", "slug": "string", "timeZoneId": "Europe/Bucharest" },
    "admin": { "name": "string", "email": "string", "password": "string" }
  }
  ```
- Validation:
  - `slug` is unique (case-insensitive), matches `^[a-z0-9](?:[a-z0-9-]{1,38}[a-z0-9])?$`.
  - `email` is a valid email and not already used.
  - `password` meets configured strength rules (mirror Identity options).
  - `timeZoneId` is a known IANA zone (use `TimeZoneInfo.TryConvertWindowsIdToIanaId` / `TimeZoneInfo.FindSystemTimeZoneById`).
- Behavior:
  - Within a single DB transaction: create `Tenant` (`Status=Active`), create `ApplicationUser` (email confirmed = false), assign `ClubAdmin` role, stamp `TenantId`.
  - Emit domain events: `TenantProvisioned`, `MemberCreated` (but member proper comes in P3 — for now create a minimal `Member` placeholder or defer until P3 and just create the user).
  - Send an email-verification email (stub; real provider in Phase 6). In dev, log the verification link to console.
- Endpoint `POST /api/v1/auth/email:verify` accepts `{ token }` and flips `EmailConfirmed=true`.
- Endpoint `GET /api/v1/auth/slug:available?slug=foo` returns `{ available: bool }` for UX.

### Out of scope

- Member full domain → **P3-T01/T02**.
- Captcha → V2.
- Billing / plan selection → V2.
- Mandatory email verification before login → for MVP, sign-in is allowed without verification but a banner is shown; the verify endpoint exists but isn't blocking. Document.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)
- Env vars:
  - `Onboarding__DefaultPlan=Free` (placeholder for V2 billing)
  - `Onboarding__VerificationTokenLifetimeHours=72`

## 5. Deliverables

- `backend/src/SlotSmart.Application/Features/Onboarding/Commands/SignupClub/SignupClubCommand.cs`
- ... handler, validator, DTOs.
- `backend/src/SlotSmart.Application/Features/Onboarding/Commands/VerifyEmail/...`
- `backend/src/SlotSmart.Application/Features/Onboarding/Queries/CheckSlugAvailability/...`
- `backend/src/SlotSmart.Api/Endpoints/OnboardingEndpoints.cs`
- Update OpenAPI examples for these endpoints.
- Tests for happy + sad paths (duplicate slug, weak password, invalid timezone).
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] Posting valid data returns `201 Created` with `{ tenant: { id, slug }, tokens: { access_token, refresh_token, expires_in } }`.
- [ ] Duplicate slug returns `409 Conflict` with `type=slotsmart/errors/slug-already-taken`.
- [ ] Invalid timezone returns `400` with field-level error.
- [ ] The new admin can immediately call `/api/v1/me` with the returned access token.
- [ ] `GET /api/v1/auth/slug:available?slug=foo` returns `200 { "available": true|false }` without requiring auth.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI updated; frontend client regenerated.
- [ ] CHANGELOG.md updated.
- [ ] Audit log entries: `ClubSignedUp`, `EmailVerificationSent` (verified via P2-T06 test).

## 8. Handoff notes / gotchas

- The signup transaction creates BOTH the tenant and the user. `ITenantContext` is not yet resolved when this endpoint runs, so the handler must explicitly pass `tenantId` to the interceptor — easiest path is to **set `TenantId` on the entity directly inside the handler** and bypass the auto-stamping for this single case via a method on the interceptor.
- Slug validation must be **case-insensitive unique**. Add a unique index on `lower(slug)` in the migration.
- The verification email link must NOT include the token in a click-tracking redirect. Format: `https://<slug>.slotsmart.app/verify?token=...`.
- Future-proof: keep `Plan` and `Locale` fields on `Tenant` even though they're unused in MVP.

## 9. Suggested execution outline

1. Add the command/query/handlers + validators.
2. Add the endpoint group.
3. Add the unique index migration on `lower(slug)`.
4. Wire the email "send" call to a `NullEmailSender` in dev (logs to console).
5. Tests.
6. OpenAPI snapshot + client regen.
7. CHANGELOG.

## 10. Open questions / risks

- Question: rate limit signup? **Decision**: yes — per IP, 5/hour, 50/day. Use the built-in ASP.NET Core rate limiter.
- Risk: a malicious party squats valuable slugs. **Mitigation (post-MVP)**: reserved slugs list + admin slug release; for MVP, accept the risk.
