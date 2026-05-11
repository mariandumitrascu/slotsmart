# Task `P2-T06` — Audit Log Infrastructure

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: S
> **Depends on**: P2-T02
> **Can run in parallel with**: P2-T03, P2-T05

---

## 1. Context

Many features will produce audit-worthy events: signups, sign-ins, role changes, booking cancellations, etc. Rather than scattering ad-hoc logging, we set up an **append-only audit log** that domain code can write to with one line. The implementation is intentionally simple in MVP (a DB table); we can swap the backend for an event store later.

## 2. Goal

> Calling `auditLog.RecordAsync(new AuditEntry(...))` from a handler persists an audit row scoped to the current tenant + user, and the entries are visible via `GET /api/v1/admin/audit` to `ClubAdmin`.

## 3. Scope

### In scope

- `AuditEntry` domain aggregate (or just an entity in `Infrastructure/Audit/`):
  - `Id` (UUIDv7), `TenantId`, `OccurredAt`, `Actor` (`UserId`, `MemberId?`, `Email`), `Action` (string code like `"member.role.changed"`), `Subject` (`Type`, `Id`), `Metadata` (jsonb), `IpAddress`, `UserAgent`.
- `IAuditLog` abstraction in `Application/Common/Abstractions/`.
- `EfAuditLog` implementation in `Infrastructure/Audit/` writing to a multi-tenant table with the standard query filter applied.
- A migration adding the `AuditEntries` table.
- Wire actor population via `ICurrentUser` abstraction reading from `HttpContext.User`.
- A small set of audit events from P2 are wired:
  - `auth.signup.club` (anonymous; tenant from new tenant id; actor = new user)
  - `auth.signin.succeeded`
  - `auth.signin.failed`
  - `auth.token.refreshed`
  - `auth.token.revoked`
- Endpoint `GET /api/v1/admin/audit` (ClubAdmin policy) with cursor pagination + filters by action prefix + date range.

### Out of scope

- A separate event store (Postgres logical-decoding, Kafka, etc.) → V2 if needed.
- Tamper-evidence / append-only enforcement at the DB level → V2.
- Long-term retention policy (TTL, archive to cold storage) → V2.
- Full UI for audit → after Phase 5.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
- Env vars: none.

## 5. Deliverables

- `backend/src/SlotSmart.Application/Common/Abstractions/IAuditLog.cs`
- `backend/src/SlotSmart.Application/Common/Abstractions/ICurrentUser.cs`
- `backend/src/SlotSmart.Infrastructure/Audit/AuditEntry.cs`
- `backend/src/SlotSmart.Infrastructure/Audit/EfAuditLog.cs`
- `backend/src/SlotSmart.Infrastructure/Identity/HttpCurrentUser.cs`
- Migration `AddAuditEntries`.
- `backend/src/SlotSmart.Api/Endpoints/AdminAuditEndpoints.cs`
- Tests: writing & querying entries respects tenancy; failed-signin entries land with `actor.userId=null` and `metadata.email`.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] A successful signup creates an `auth.signup.club` audit entry visible in the new tenant's admin audit listing.
- [ ] A failed signin (wrong password) creates an `auth.signin.failed` entry with no `userId` but with the attempted email in `metadata`.
- [ ] Cross-tenant: an admin in tenant A cannot see tenant B's audit entries.
- [ ] `GET /api/v1/admin/audit?action=auth.&from=...&to=...` returns paginated results, newest first.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI updated; client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- For failed-signin events the `TenantId` may be unknown (the email doesn't match any user). **Decision**: write to a special `null` tenant scope OR write to all tenants matching the email. **MVP simplification**: only write if a tenant can be inferred; otherwise log to Serilog only. Document.
- The `Metadata` column is `jsonb`. Build it from a small DTO and let EF Core convert (`HasColumnType("jsonb")`).
- Make `IAuditLog.RecordAsync` non-throwing in the happy path; if the write fails, log the error and continue — audit failures must not block business logic. The exception is the actual signup transaction, where the audit row is part of the same `SaveChangesAsync` as the tenant/user.

## 9. Suggested execution outline

1. Add the abstractions and the EF implementation.
2. Add the migration.
3. Wire `ICurrentUser` in `Program.cs`.
4. Sprinkle the five auth events.
5. Add the admin endpoint.
6. Tests + CHANGELOG.

## 10. Open questions / risks

- Question: do we expose audit to non-admin members? **Decision (MVP)**: no.
- Risk: a chatty handler could fill the audit table. **Mitigation**: keep audit calls intentional. Add `pg_partman` partitioning post-MVP if volume grows.
