# Phase Gate G3 — Club & Members → Training

> **Gate purpose**: Verify the **people-management** layer is complete and tenant-safe.
> **Source phase plan**: [`../../plan/phase-3-club-members/README.md`](../../plan/phase-3-club-members/README.md)
> **Status**: ⏳ Not yet run

---

## 1. Pre-conditions

- [ ] **P3-T01** Club settings domain & endpoints
- [ ] **P3-T02** Member aggregate + CRUD + invitations
- [ ] **P3-T03** Role management on Member
- [ ] **P3-T04** Coach profile extension
- [ ] **P3-T05** Parent–Child relations (MemberRelation)
- [ ] **P3-T06** Frontend: club settings, member directory, invitations
- [ ] **P3-T07** Frontend: coach profiles + parent/child management

---

## 2. Automated verification

(Pre-req: Phase 2 gate passed; an admin token is available — `$TOKEN` from G2 §2.3.)

### 2.1 Club settings round-trip

```bash
curl -fsS -X PUT http://localhost:5080/api/v1/clubs/me/settings \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"timeZone":"Europe/Bucharest","defaultBookingWindowHours":48,"defaultCancellationWindowHours":12}' \
  | jq .

curl -fsS http://localhost:5080/api/v1/clubs/me/settings -H "Authorization: Bearer $TOKEN" | jq .
```

**Expected**: 200; PUT response equals subsequent GET response.

### 2.2 Invite a member by email

```bash
curl -fsS -X POST http://localhost:5080/api/v1/members:invite \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"email":"player1@acme.test","name":"Player One","roles":["Player"]}' | jq .
```

**Expected**: 201; response includes `invitationToken` (or links to retrieving it). The invitee can `POST /api/v1/auth/invitations:accept` and become an active member.

### 2.3 Role change reissues claims on next sign-in

```bash
# Change role via admin endpoint
curl -fsS -X PUT http://localhost:5080/api/v1/members/{memberId}/roles \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"roles":["Player","Coach"]}'
# Sign in again → token contains the new roles claim
```

**Expected**: New JWT contains both `Player` and `Coach` role claims. Phase scope explicitly does **not** include live revocation.

### 2.4 Deactivation prevents sign-in

```bash
curl -fsS -X PUT http://localhost:5080/api/v1/members/{memberId}/status \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"isActive":false}'
# Sign in attempt → 401 / 403 with "account_disabled" code
```

**Expected**: signin attempt fails with the documented error.

### 2.5 Coach profile

```bash
curl -fsS -X PUT http://localhost:5080/api/v1/coaches/{memberId} \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"specialization":"Junior development","bio":"...","photoUrl":"https://..."}'

curl -fsS http://localhost:5080/api/v1/coaches/{memberId} -H "Authorization: Bearer $TOKEN" | jq .
```

**Expected**: GET reflects PUT.

### 2.6 Parent–Child relation

```bash
curl -fsS -X POST http://localhost:5080/api/v1/members/{parentId}/children \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"childMemberId":"{childId}","relationType":"Parent"}'

curl -fsS http://localhost:5080/api/v1/members/{parentId}/children -H "Authorization: Bearer $TOKEN" | jq .
```

**Expected**: child appears in the list; reverse lookup `/members/{childId}/parents` also works.

### 2.7 Cross-tenant access still returns 404

```bash
# Use $TOKEN_CLUB_A but a memberId that belongs to club B
curl -s -o /dev/null -w '%{http_code}\n' \
  http://localhost:5080/api/v1/members/{otherClubMemberId} \
  -H "Authorization: Bearer $TOKEN_CLUB_A"
```

**Expected**: `404`.

### 2.8 Integration tests

```bash
cd backend && dotnet test --filter "Category=Integration"
```

**Expected**: All Phase 3 integration tests pass (Testcontainers Postgres).

---

## 3. Manual verification (frontend)

- [ ] Member directory loads, paginates, filters by role.
- [ ] Invite-by-email flow sends an invitation token and lands the invitee on a "set password" screen.
- [ ] Coach profile page is editable and renders bio/specialization.
- [ ] Parent's "My children" page shows linked child Players.

---

## 4. Promotion checklist

- [ ] §1 tasks `✅ COMPLETED`
- [ ] §2 commands clean
- [ ] §3 manual checks confirmed
- [ ] CHANGELOG entries for all P3 tasks
- [ ] THINKING-LOG: "Phase 3 closed"

---

## 5. Run log

| Date | Result | Evidence | Notes |
|---|---|---|---|
| _empty_ | — | — | — |

---

**Gate version**: 1.0
