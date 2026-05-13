# Phase Gate G2 — Auth & Tenancy → Club & Members

> **Gate purpose**: Verify the platform is **secure** and **tenant-aware** before feature work begins.
> **Source phase plan**: [`../../plan/phase-2-auth-tenancy/README.md`](../../plan/phase-2-auth-tenancy/README.md)
> **Status**: ⏳ Not yet run

---

## 1. Pre-conditions (task completion)

- [ ] **P2-T01** Tenant resolution & EF query filters
- [ ] **P2-T02** Authentication: OpenIddict + JWT + refresh tokens
- [ ] **P2-T03** Role-based access control (RBAC) end-to-end
- [ ] **P2-T04** Club signup & first admin onboarding
- [ ] **P2-T05** Frontend auth flow (sign up, sign in, logout, refresh)
- [ ] **P2-T06** Audit log infrastructure

---

## 2. Automated verification

Phase 1 stack must be running (`docker compose up`). Use a fresh DB volume.

### 2.1 Architecture / authorization rules enforced at compile/test time

```bash
cd backend
dotnet test --filter "FullyQualifiedName~Architecture"
```

**Expected**: Test asserting "every endpoint has `[Authorize(policy=...)]` or `[AllowAnonymous]`" passes (P2 acceptance criteria).

### 2.2 Anonymous club signup

```bash
curl -fsS -X POST http://localhost:5080/api/v1/auth/clubs:signup \
  -H 'Content-Type: application/json' \
  -d '{
    "clubName":"Acme Tennis",
    "slug":"acme",
    "adminEmail":"admin@acme.test",
    "adminPassword":"P@ssw0rd!2026",
    "adminName":"Admin One"
  }'
```

**Expected**: HTTP 201 Created with `Location` header. Body includes `clubId`, `userId`, and (per plan) the role `ClubAdmin`.

### 2.3 Token issuance (password grant)

```bash
TOKEN=$(curl -fsS -X POST http://localhost:5080/api/v1/auth/token \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'grant_type=password&username=admin@acme.test&password=P%40ssw0rd%212026&scope=offline_access' \
  | jq -r .access_token)
echo "TOKEN length: ${#TOKEN}"
```

**Expected**: `TOKEN length` > 100. Refresh token also returned.

### 2.4 Authenticated `me` endpoint

```bash
curl -fsS http://localhost:5080/api/v1/me -H "Authorization: Bearer $TOKEN" | jq .
```

**Expected**: JSON includes `email`, `roles` containing `ClubAdmin`, `tenantId`/`clubId` set.

### 2.5 Unauthenticated access blocked except allow-list

```bash
# Should be 401
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5080/api/v1/me
# Should be 200/204 (allow-listed)
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5080/api/v1/health
```

**Expected**: `401` then `200`.

### 2.6 Cross-tenant isolation returns 404 (no leak)

Sign up a second club, fetch a resource with the first club's token using the second club's ID — must return `404`, not `403` (per phase acceptance: "does not leak existence"). Concrete script depends on what entities exist by end of Phase 2 (likely just `Member` self / `Club` self).

```bash
# pseudo: scripted by the worker who ships P2-T03; included in their completion report
```

### 2.7 Audit log captures the documented events

```bash
# Issue the events
curl -X POST .../auth/clubs:signup ...        # MemberCreated
curl -X POST .../auth/token ...               # SignInSucceeded
curl -X POST .../auth/token -d '...wrong...'  # SignInFailed

# Query DB or audit endpoint
psql "$DB_URL" -c "select event_type, count(*) from audit_log group by 1;"
```

**Expected**: rows for `MemberCreated`, `SignInSucceeded`, `SignInFailed` (and `MemberRoleChanged` once an admin changes a role).

### 2.8 Frontend auth round-trip

```bash
# Playwright spec lives in frontend/tests/e2e/auth.spec.ts (P2-T05)
cd frontend
npx playwright test tests/e2e/auth.spec.ts
```

**Expected**: All scenarios pass: signup → signin → refresh → access protected page → logout.

---

## 3. Manual verification

- [ ] Sign up a club via the frontend; redirected to `/app` placeholder.
- [ ] Refresh token flow: leave the app open past access-token expiry; next API call transparently refreshes.
- [ ] Logout clears tokens and protected routes redirect to sign-in.
- [ ] Browsing to another club's data via crafted URL returns 404 in the network tab.

---

## 4. Promotion checklist

- [ ] All §1 tasks `✅ COMPLETED`
- [ ] All §2 commands clean (evidence captured)
- [ ] All §3 manual checks confirmed
- [ ] CHANGELOG entries for all P2 tasks
- [ ] DECISIONS-LOG ADRs for: token TTLs, refresh-token rotation policy, audit-log storage choice
- [ ] THINKING-LOG entry "Phase 2 closed"

---

## 5. Run log

| Date | Result | Evidence | Notes |
|---|---|---|---|
| _empty_ | — | — | — |

---

**Gate version**: 1.0
