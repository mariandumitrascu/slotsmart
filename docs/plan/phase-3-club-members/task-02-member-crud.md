# Task `P3-T02` — Member Aggregate + CRUD + Invitations

> **Phase**: 3 — Club & Member Management
> **Estimated size**: L
> **Depends on**: P3-T01
> **Can run in parallel with**: nothing major

---

## 1. Context

Phase 2 created the `ApplicationUser` (sign-in) but no full `Member` profile. We add the `Member` aggregate, link 1:1 to `ApplicationUser` per tenant, and add the invitation flow that lets admins onboard people without sharing passwords.

## 2. Goal

> A Club Admin can invite, view, list, update, deactivate, and (soft) delete members. An invited person receives a tokenized link, accepts it, sets a password, and joins the club as a Member with the assigned roles.

## 3. Scope

### In scope

- Aggregate `Member` in `Domain/Members/`:
  - `Id`, `TenantId`, `UserId?` (nullable until invitation accepted), `Email`, `FirstName`, `LastName`, `Phone?`, `DateOfBirth?`, `IsMinor` (derived), `IsActive`, `Roles` (collection of `Role` enum), `CreatedAt`, `UpdatedAt`, `DeactivatedAt?`, `Version`.
  - Invariants:
    - At least one role required at all times.
    - `Email` unique per tenant (case-insensitive).
    - `IsMinor` re-derived on profile change.
- Aggregate `MemberInvitation`:
  - `Id`, `TenantId`, `MemberId` (created in `Invited` state), `TokenHash`, `ExpiresAt`, `AcceptedAt?`.
- Use cases:
  - `InviteMember` (Club Admin): create `Member` in `Invited` state with `UserId=null`, create `MemberInvitation`, send invitation email (stub via `IEmailSender`).
  - `AcceptInvitation` (anonymous): exchange token for password setting; creates the `ApplicationUser`, sets `Member.UserId`, marks invitation accepted, returns token pair.
  - `ListMembers` (Coach or above): filter by role, name, status; paginated.
  - `GetMember` (any member can read others within the tenant; PII fields restricted to Coach+).
  - `UpdateMember` (Club Admin always; member can update own profile fields).
  - `DeactivateMember` (Club Admin).
  - `ReactivateMember` (Club Admin).
  - `DeleteMember` (Club Admin) — soft delete; prevents future bookings; historical bookings retained.
- Validators (FluentValidation) per command.
- Authorization:
  - Reading another member's PII (phone, DOB) requires Coach+.
  - Updating another member requires Club Admin.
  - Self-update allowed for `FirstName`, `LastName`, `Phone`, `DateOfBirth`.
- Audit entries for every action.

### Out of scope

- Role assignment endpoint → **P3-T03**.
- Coach-specific fields → **P3-T04**.
- Parent–child links → **P3-T05**.
- Email template rendering → Phase 6 (Notifications). Use a minimal text template + stub `IEmailSender` from P2.

## 4. Inputs

- Architecture docs (all of `00-architecture`).
- Env vars: `Invitations__TokenLifetimeHours=72`.

## 5. Deliverables

- `backend/src/SlotSmart.Domain/Members/Member.cs`, `MemberStatus.cs`, value objects (`Email`, `PhoneNumber`, `PersonName`).
- `backend/src/SlotSmart.Domain/Members/MemberInvitation.cs`.
- EF configurations + migration `AddMembers`.
- Application use cases + endpoints in `MembersEndpoints.cs`.
- Audit entries: `member.invited`, `member.invitation.accepted`, `member.updated`, `member.deactivated`, `member.reactivated`, `member.deleted`.
- Tests:
  - Domain: invariants (≥1 role, unique email).
  - Application: each use case happy + sad paths.
  - Endpoint: cross-tenant isolation; non-admin cannot list PII fields.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] `POST /api/v1/members:invite` creates a member in `Invited` state and produces an invitation token (in dev: logged to console).
- [ ] `POST /api/v1/auth/invitations:accept` with `{ token, password, firstName, lastName, dateOfBirth? }` creates the user, links to the member, returns token pair.
- [ ] `GET /api/v1/members?role=Coach&q=...&page=1` returns paginated results.
- [ ] `GET /api/v1/members/{id}` returns full details for Coach+, redacted PII for peers (Player viewing another Player sees only name + role).
- [ ] `PATCH /api/v1/members/{id}` works for self (own fields) and for Club Admin (any field except roles — those have a dedicated endpoint per T03).
- [ ] `POST /api/v1/members/{id}:deactivate` flips `IsActive=false`; the user cannot sign in until reactivated.
- [ ] Invitation token expired → 410 Gone.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.
- [ ] All endpoints have audit entries.

## 8. Handoff notes / gotchas

- `Email` value object: enforces RFC-5321 basic shape + lowercased canonical form for uniqueness.
- `IsMinor` is derived from `DateOfBirth` against the tenant's locale (assume 18 in MVP; can be configured later).
- Soft delete uses an `IsDeleted` column with the EF query filter combining tenant filter AND `!IsDeleted`.
- The invitation token is stored as a **hash** (`Sha256(token)`), not plaintext.
- The accept-invitation endpoint is `[AllowAnonymous]` and `[AllowAnonymousTenant]`; tenant is derived from the invitation's stored `TenantId`.

## 9. Suggested execution outline

1. Aggregate + value objects + invariants + tests (Domain only first).
2. EF configurations + migration.
3. Application use cases + validators.
4. Endpoints + policies.
5. Audit entries.
6. Integration tests.
7. OpenAPI + client regen.
8. CHANGELOG.

## 10. Open questions / risks

- Question: do we let people register themselves directly (without an invitation)? **Decision (MVP)**: no — onboarding is invite-only after the founding signup. This protects clubs from random sign-ups. Public sign-up of *clubs* still works.
- Risk: an invited person already has a SlotSmart account at a different tenant. **MVP simplification**: a User is single-tenant, so the accept endpoint will reject if the email is in use elsewhere. V2 supports multi-tenant users.
