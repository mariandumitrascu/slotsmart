# Handoff Prompt — P1-T02 Database, EF Core & Migrations Bootstrap

## Dispatch metadata (SUPERVISOR / user-facing — do **not** copy to the worker)

- **Suggested model**: **Sonnet** (balanced default)
- **Why this model**: The task is well-specified (canonical `task-02-database-ef-core.md`) and mostly mechanical infrastructure wiring. The only architecturally-loaded part is the `EntityConfiguration<T>` base class for the dual-key pattern, which is also fully described in `entity-identity.md`. Sonnet handles this at lower cost than Opus.
- **Suggested escalation**: Switch to **Opus** if any of these surface:
  - Testcontainers-Postgres misbehaves on the host (Apple Silicon edge cases) and the worker has to design a fallback.
  - The worker proposes a substantive change to `ITenantContext` shape beyond `entity-identity.md` §4.
- **Do not use Auto**: This task sets EF Core conventions (configuration base, interceptor pattern, migration assembly, tenant-stamping wiring) that every later persistence task inherits.

## Updates since the original task spec was written (2026-05-13)

- **ADR-007 (dual-key identity pattern)** is in force. The original spec lists `IUnitOfWork` / `ITenantContext` / `IClock` abstractions in `Application/Common/Abstractions/` — that's still correct, BUT `ITenantContext` must carry **both** the surrogate `long TenantId` and the public `Guid TenantEntityId` per `entity-identity.md` §4 + `multi-tenancy-strategy.md` §3.
- **`EntityConfiguration<TEntity>` base class** (`entity-identity.md` §2) MUST be created in P1-T02 — it adds the surrogate `bigint` shadow PK and the unique index on `EntityId`. Every concrete configuration in later phases inherits from it.
- **.NET 10 GA (10.0.300, LTS)** is installed; EF Core 10 GA is available. Pin `Npgsql.EntityFrameworkCore.PostgreSQL` to the latest 10.x stable, `Microsoft.EntityFrameworkCore.Design` to match.
- **`dotnet-ef`** is to be installed as a **local** tool (`backend/.config/dotnet-tools.json`) so the version is pinned per repo. The `.gitignore` exemption for that file is already in place.
- **P1-T01 deliverables** that you can build on:
  - `SlotSmart.Domain.Common.Entity` (public `EntityId : Guid` with `protected init`); marker interfaces `IAggregateRoot`, `IDomainEvent`, `IMultiTenantEntity { long TenantId }`.
  - `SlotSmart.Shared.Identifiers.UuidV7.NewGuid()` for any seed data you need.
  - `SlotSmart.Application.DependencyInjection.AddSlotSmartApplication(...)` and `SlotSmart.Infrastructure.DependencyInjection.AddSlotSmartInfrastructure(...)` exist as no-op stubs — extend them, don't recreate.
  - Architecture tests exist for layer rules and dual-key entity rules — your work must keep them green.

---

## ⤵ COPY EVERYTHING BELOW THIS LINE TO THE WORKER ⤵

# Handoff — P1-T02 Database, EF Core & Migrations Bootstrap

You are a **WORKER** agent. Your job: implement P1-T02 as specified in [`docs/plan/phase-1-foundation/task-02-database-ef-core.md`](../../plan/phase-1-foundation/task-02-database-ef-core.md). Do not invent scope.

## 1. Read these files before writing any code

In this order:

1. [`docs/SUPERVISOR/WORKER-FRAMEWORK.md`](../WORKER-FRAMEWORK.md) — your operating contract.
2. [`docs/plan/00-architecture/solution-structure.md`](../../plan/00-architecture/solution-structure.md) — Infrastructure layout.
3. [`docs/plan/00-architecture/entity-identity.md`](../../plan/00-architecture/entity-identity.md) — **REQUIRED**: dual-key pattern. The `EntityConfiguration<T>` base class you'll create here is the linchpin of every future persistence configuration.
4. [`docs/plan/00-architecture/multi-tenancy-strategy.md`](../../plan/00-architecture/multi-tenancy-strategy.md) — `ITenantContext` shape + interceptor responsibilities.
5. [`docs/plan/00-architecture/coding-standards.md`](../../plan/00-architecture/coding-standards.md).
6. [`docs/plan/phase-1-foundation/task-02-database-ef-core.md`](../../plan/phase-1-foundation/task-02-database-ef-core.md) — your canonical spec. **This is the source of truth for scope, deliverables, and acceptance criteria.**

## 2. State of the repo at the moment of this handoff

| Item | Status | Notes |
| --- | --- | --- |
| .NET SDK | 10.0.300 (GA, LTS) | Pinned in `backend/global.json` |
| Solution | `backend/SlotSmart.slnx` (10 projects) | Build clean, 0 warnings |
| Domain | `Entity` base + 3 marker interfaces; **no concrete entities yet** | Per ADR-007 |
| Application | `DependencyInjection.AddSlotSmartApplication()` exists as no-op stub | Extend it |
| Infrastructure | `DependencyInjection.AddSlotSmartInfrastructure()` exists as no-op stub | Extend it (will become `AddInfrastructure(IConfiguration)` per spec §5) |
| Api | Minimal API; `/api/v1/health` returns 200; calls both DI methods | Add migration-apply on startup in Development |
| Tests | 5 projects, 15 tests, all green | Add 1+ integration test per spec §5 |
| Architecture tests | LayerRules + EntityIdentity (4 dual-key rules) | Must stay green; consider adding a new arch test for "every multi-tenant entity has a query filter" (it's vacuously true today since none exist; the test wires up the framework for later) |
| `dotnet-ef` | NOT yet installed | Install as **local** tool: `dotnet new tool-manifest --output backend && dotnet tool install --add-source local dotnet-ef --tool-manifest backend/.config/dotnet-tools.json` (or simply `cd backend && dotnet new tool-manifest && dotnet tool install dotnet-ef`) |
| Docker | Available; `postgres:16-alpine` already pulled | Testcontainers will be fast |

## 3. Specific deviations / clarifications from the canonical spec

These are SUPERVISOR-approved deviations or clarifications since the spec was written:

1. **`ITenantContext` shape**: must match `multi-tenancy-strategy.md` §3 exactly:
   ```csharp
   public interface ITenantContext
   {
       long TenantId { get; }            // surrogate bigint — used by query filter and FKs
       Guid TenantEntityId { get; }      // UUIDv7 — used in JWT, audit, public URLs
       string TenantSlug { get; }
       bool IsResolved { get; }
   }
   ```
   The `NoopTenantContext` you ship in this task returns `TenantId = 0`, `TenantEntityId = Guid.Empty`, `TenantSlug = ""`, `IsResolved = false`. Real resolution lands in P2-T01.

2. **`EntityConfiguration<TEntity>` base class** (NEW deliverable not in the original spec — required by ADR-007):
   - Location: `backend/src/SlotSmart.Infrastructure/Persistence/Configurations/EntityConfiguration.cs`.
   - Adds shadow `Id : long` property as PK with `ValueGeneratedOnAdd().UseIdentityColumn()`.
   - Configures `EntityId` as `ValueGeneratedNever().IsRequired()` with a unique index.
   - Concrete configurations call `base.Configure(builder)` first.
   - Add an architecture test that asserts every `IEntityTypeConfiguration<>` implementer in `SlotSmart.Infrastructure` derives from `EntityConfiguration<>`. (It will pass vacuously today; will catch violations in P3.)

3. **No entities yet → no migration content**: the `InitialCreate` migration should produce a no-op `Up()` / `Down()` (or just the `__EFMigrationsHistory` bootstrap). That is expected and acceptance-criteria-compliant.

4. **Connection string**: `appsettings.Development.json` uses `Host=localhost;Port=5432;Database=slotsmart;Username=slotsmart;Password=slotsmart;Include Error Detail=true`. NOT a real password — local-only. README documents the `docker run --rm -d --name slotsmart-pg -e POSTGRES_USER=slotsmart -e POSTGRES_PASSWORD=slotsmart -e POSTGRES_DB=slotsmart -p 5432:5432 postgres:16-alpine` one-liner so devs can stand it up without docker compose (which lands in P1-T03).

5. **Migrations on startup**: `app.Environment.IsDevelopment()`-gated. Production must NOT auto-apply. Wrap in a try/catch that logs the failure but does not crash the host (so dev-mode wiring problems are obvious without taking down the API).

6. **`AddDbContext` not `AddDbContextPool`**: per spec gotcha — pooling waits until we have load.

## 4. Acceptance you must meet (mirrors spec §6 + supervisor additions)

- [ ] `cd backend && dotnet build SlotSmart.slnx -warnaserror` → 0 warnings, 0 errors.
- [ ] `cd backend && dotnet test SlotSmart.slnx` → all tests pass, including the new integration test.
- [ ] `cd backend && dotnet ef migrations add InitialCreate -p src/SlotSmart.Infrastructure -s src/SlotSmart.Api -o Persistence/Migrations` re-runs cleanly (i.e., the committed migration matches a fresh generation modulo timestamp).
- [ ] `dotnet ef database update -p src/SlotSmart.Infrastructure -s src/SlotSmart.Api` against a local Postgres (use the docker one-liner) applies the migration.
- [ ] `dotnet run --project src/SlotSmart.Api --urls http://localhost:5080` against a fresh local Postgres applies migrations on startup AND `curl /api/v1/health` still returns 200.
- [ ] `DbContextSmokeTests` runs in <30s, asserts migrations applied, and demonstrates the SaveChanges interceptor was invoked (assert via a test spy / counting interceptor or by replacing `TenantStampingInterceptor` with a spying wrapper in the test fixture).
- [ ] `architecture-tests`: existing 9 stay green; new test "every IEntityTypeConfiguration<> in Infrastructure derives from EntityConfiguration<>" added and green.
- [ ] No connection strings with real credentials in source. Local-only `appsettings.Development.json` + env-var override `Postgres__ConnectionString`.
- [ ] `CHANGELOG.md` updated under `[Unreleased] / Added`.
- [ ] `README.md` "Build & run" updated with the Postgres docker one-liner + `dotnet ef` invocation.

## 5. Definition of Done

All acceptance boxes ticked + completion report posted in this format:

```markdown
# WORKER COMPLETION REPORT — P1-T02

## Status: [ COMPLETED / BLOCKED ]
## Files added
- (list)
## Files modified
- (list)
## Verification evidence
- dotnet build: <output>
- dotnet test: <output>
- dotnet ef commands: <output>
- live API + curl: <output>
## Acceptance criteria
- [x] <each>
## Open follow-ups for next worker
- (any new TODOs that fall out)
## Notes / deviations
- (anything you decided differently and why)
```

## 6. Out of scope (do NOT do these)

- Any business aggregates (Tenant, Member, Training, Booking, etc.) — those land in their own phase tasks.
- Real tenant query filter activation — `NoopTenantContext` returns `TenantId = 0`; the query filter on a future `IMultiTenantEntity` will simply match nothing in this task. The **filter wiring** in `OnModelCreating` is in scope; the **entities to filter** are not.
- Outbox table or polling — P5.
- Audit / soft-delete interceptors — P2-T03 / P2-T06.
- OpenIddict / Identity tables — P2-T02.

## 7. Working agreements

- One PR / one task. Don't bundle other phase 1 tasks.
- Use bash, not zsh/PowerShell, in any documented commands.
- Follow the Keep-a-Changelog 1.1.0 format in CHANGELOG.md.
- If you discover an issue with the task spec or an upstream architecture doc, FLAG it in your completion report — do not silently deviate beyond §3 above.

Good luck — when complete, hand the report back to the SUPERVISOR for verification against the Phase 1 gate.
