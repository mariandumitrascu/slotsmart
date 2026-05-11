# Task `P2-T05` — Frontend Auth Flow

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: L
> **Depends on**: P2-T02 (token endpoint); P2-T04 useful but can be stubbed
> **Can run in parallel with**: P2-T06

---

## 1. Context

The React app needs the auth surface that makes the rest of the product possible: a public area (sign up, sign in), a protected area (`/app/*`), token storage, automatic refresh, and graceful logout.

## 2. Goal

> A first-time visitor can sign up a club, gets routed into the app, and stays signed in across page reloads. An access-token expiry triggers a transparent refresh; if refresh fails, the user is redirected to sign-in.

## 3. Scope

### In scope

- Routes:
  - `/` — marketing/landing placeholder.
  - `/signup` — multi-step form (club info → admin info → confirmation).
  - `/signin` — email + password.
  - `/verify?token=...` — email verification.
  - `/app/*` — protected layout (the existing app shell from P1-T05 is moved under this prefix). Default landing `/app` placeholder dashboard.
- `useAuth` hook (built on Zustand) with: `user`, `tenant`, `roles`, `accessToken`, `signIn`, `signUp`, `signOut`, `refresh`, `hasRole(...)`.
- Token storage:
  - **Access token** in memory (Zustand).
  - **Refresh token** in `localStorage` only after explicit "remember me" choice; otherwise `sessionStorage`. Document the threat model.
- `apiClient` integration:
  - Attaches `Authorization: Bearer <accessToken>` on every protected request.
  - On `401` + `WWW-Authenticate: error="invalid_token"`, performs a single refresh and retries; if refresh fails, signs out.
- Forms: React Hook Form + Zod, with field-level validation messages translated.
- `<ProtectedRoute>` component that redirects to `/signin?returnTo=...` if unauthenticated.
- Role-aware navigation: nav items hidden if the user lacks the required role(s); `useRequiredRole(role)` helper for in-component checks.
- E2E test (Playwright) covering: signup → land on `/app` → reload → still signed in → sign out → can't access `/app`.

### Out of scope

- "Forgot password" UI → small follow-up after P2-T04.
- Social sign-in → V2.
- MFA → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Env vars introduced:
  - none new.
- API contracts (from P2-T02, P2-T04):
  - `POST /api/v1/auth/token`, `POST /api/v1/auth/token/refresh`, `POST /api/v1/auth/clubs:signup`, `GET /api/v1/auth/slug:available`, `GET /api/v1/me`.

## 5. Deliverables

- `frontend/src/features/auth/` containing:
  - `pages/SignInPage.tsx`, `pages/SignUpPage.tsx`, `pages/VerifyEmailPage.tsx`.
  - `components/SignUpStepClub.tsx`, `SignUpStepAdmin.tsx`, `SignUpConfirmation.tsx`.
  - `api/auth.api.ts` (TanStack mutations).
  - `state/authStore.ts` (Zustand).
  - `hooks/useAuth.ts`, `hooks/useRequiredRole.ts`.
- `frontend/src/lib/apiClient.ts` updated with refresh-on-401 logic.
- `frontend/src/app/routes.tsx` reorganized for public vs `/app/*` protected sections.
- `frontend/src/app/layout/AppShell.tsx` adjusted to show user menu + sign-out.
- E2E test under `frontend/tests/auth.spec.ts`.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] A user can sign up a club from `/signup`, sees a success state, and lands on `/app`.
- [ ] A user can sign in from `/signin` and lands on `/app` (or `returnTo`).
- [ ] Reloading `/app` while signed in does not redirect to `/signin`.
- [ ] An expired access token (simulated by clearing it from store) triggers a single refresh and request retry; success goes through silently.
- [ ] After sign-out, `/app` redirects to `/signin?returnTo=/app`.
- [ ] Nav links for "Members admin" are hidden when the user is not `ClubAdmin` (verified once role-gated pages exist; for now stub a placeholder link guarded by `hasRole('ClubAdmin')`).
- [ ] Playwright E2E test passes locally.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] No `any` in auth code.
- [ ] All strings i18n'd through `t(...)`.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Putting the refresh token in `localStorage` is convenient but exposed to XSS. With our CSP and React's escaping, this is acceptable for MVP; document. The proper long-term solution is HttpOnly cookies + same-site lax + a CSRF strategy — V2.
- Make sure to **dedupe** concurrent refresh attempts in `apiClient`: if 5 requests hit a 401 simultaneously, only one refresh should fire, and the other four queue on its result.
- On sign-out, also revoke the refresh token via the backend (`POST /auth/token/revoke` — add it if not present).
- Slug suggestion: on the signup form, debounce a call to `/auth/slug:available` and show inline availability.

## 9. Suggested execution outline

1. Restructure routes into `public` and `protected` layouts.
2. Build the `authStore` with the documented persistence policy.
3. Implement `apiClient` interceptor for refresh-on-401 with dedupe.
4. Build sign-in form, then sign-up wizard.
5. Wire `<ProtectedRoute>`, sign-out, user menu.
6. Add Playwright E2E for the happy path.
7. CHANGELOG.

## 10. Open questions / risks

- Question: do we ship the verify-email flow in MVP? **Decision**: ship the UI; it's gentle (non-blocking).
- Risk: the refresh logic is the most subtle part of any frontend. **Mitigation**: small, well-tested module with explicit dedupe and unit tests for the queue logic.
