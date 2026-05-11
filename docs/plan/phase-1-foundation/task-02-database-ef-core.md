# Task `P1-T02` — Database, EF Core & Migrations Bootstrap

> **Phase**: 1 — Foundation
> **Estimated size**: M
> **Depends on**: P1-T01
> **Can run in parallel with**: P1-T04, P1-T05, P1-T07

---

## 1. Context

We need the persistence layer in place — empty but functional — so subsequent phases can add aggregates without re-doing infrastructure. We are using PostgreSQL 16+ with EF Core 10. Multi-tenancy plumbing (query filters, tenant stamping) is set up at the **interface** level here but only activated in Phase 2 when we actually have multiple tenants.

## 2. Goal

> The API can connect to Postgres, run an empty migration, and a smoke test confirms a round-trip read/write through `SlotSmartDbContext` against a real Postgres instance.

## 3. Scope

### In scope

- Add EF Core 10 + Npgsql packages to `SlotSmart.Infrastructure` and `SlotSmart.Application` (just `Microsoft.EntityFrameworkCore` abstractions in Application).
- Create `SlotSmartDbContext` in `Infrastructure/Persistence/`.
- Define abstractions in `Application/Common/Abstractions/`:
  - `IUnitOfWork`, `ITenantContext` (no-op default in this task), `IClock` (returns `DateTimeOffset.UtcNow`).
- Implement `Infrastructure/Tenancy/NoopTenantContext.cs` as the default registration; **real** tenant resolution is P2-T01.
- Add a SaveChanges interceptor stub `TenantStampingInterceptor` (no-op for now; just wired up).
- Configure connection string via `appsettings.json` + `Postgres__ConnectionString` env var.
- Add an initial empty migration `InitialCreate` so `dotnet ef database update` succeeds.
- Add Testcontainers-Postgres-based base class for integration tests (`PostgresIntegrationTestBase`).
- One integration test in `Infrastructure.Tests` that boots `SlotSmartDbContext` against Testcontainers and verifies migrations apply.

### Out of scope

- Any business aggregates → later phases.
- Real tenant filtering → **P2-T01**.
- Migrations for business tables → respective phase tasks.
- Audit / soft delete interceptors with real logic → **P2-T03**.
- Outbox → **P5** (when domain events first need reliable dispatch).

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md) (Infrastructure layout)
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Connection string format: `Host=postgres;Port=5432;Database=slotsmart;Username=slotsmart;Password=…`.
- Env vars introduced:
  - `Postgres__ConnectionString` — overrides the value in `appsettings.json`.

## 5. Deliverables

### Backend

- `backend/src/SlotSmart.Infrastructure/Persistence/SlotSmartDbContext.cs`
- `backend/src/SlotSmart.Infrastructure/Persistence/Interceptors/TenantStampingInterceptor.cs` (no-op stub)
- `backend/src/SlotSmart.Infrastructure/Persistence/Configurations/` (empty folder + placeholder file)
- `backend/src/SlotSmart.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- `backend/src/SlotSmart.Application/Common/Abstractions/IUnitOfWork.cs`
- `backend/src/SlotSmart.Application/Common/Abstractions/ITenantContext.cs`
- `backend/src/SlotSmart.Application/Common/Abstractions/IClock.cs`
- `backend/src/SlotSmart.Infrastructure/Time/SystemClock.cs`
- `backend/src/SlotSmart.Infrastructure/Tenancy/NoopTenantContext.cs`
- `backend/src/SlotSmart.Infrastructure/DependencyInjection.cs` registers DbContext + the above.
- `backend/src/SlotSmart.Infrastructure/Persistence/Migrations/<timestamp>_InitialCreate.cs`
- `backend/src/SlotSmart.Api/Program.cs` wires `services.AddInfrastructure(configuration)` and applies migrations in dev on startup.
- `backend/src/SlotSmart.Api/appsettings.Development.json` with localhost Postgres connection string.

### Tests

- `backend/tests/SlotSmart.Infrastructure.Tests/PostgresIntegrationTestBase.cs` (Testcontainers).
- `backend/tests/SlotSmart.Infrastructure.Tests/DbContextSmokeTests.cs` — verifies migrations apply and `SaveChangesAsync` works on an empty context.

### Docs

- Update root `README.md` "How to run" with Postgres step (local Docker run command).
- Update `CHANGELOG.md` under `Added`.

## 6. Acceptance Criteria

- [ ] `dotnet ef migrations add InitialCreate -p src/SlotSmart.Infrastructure -s src/SlotSmart.Api -o Persistence/Migrations` produced the migration committed in this PR.
- [ ] `dotnet ef database update` against a local Postgres applies the migration.
- [ ] `dotnet run --project src/SlotSmart.Api` on a fresh local Postgres applies migrations automatically in Development.
- [ ] The integration test `DbContextSmokeTests` passes (downloads Postgres image once, runs in <30s).
- [ ] The `TenantStampingInterceptor` is registered and the SaveChanges path goes through it (verified by adding a debug breakpoint or test spy).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] `dotnet build` clean, `dotnet test` green.
- [ ] No connection strings with real credentials in source. Default in `appsettings.Development.json` uses a local-only password.
- [ ] CHANGELOG.md updated.
- [ ] README.md updated with the Postgres run instructions.

## 8. Handoff notes / gotchas

- Do **not** apply migrations automatically in production. Gate the call on `app.Environment.IsDevelopment()`.
- The `DesignTimeDbContextFactory` is required so EF Core tooling (`dotnet ef`) can construct the context without booting the full host.
- Use `UseNpgsql(connectionString, npg => npg.MigrationsAssembly("SlotSmart.Infrastructure"))`.
- Register the DbContext with `AddDbContextPool` for performance once we have load; for now `AddDbContext` is fine.
- Set `EnableDetailedErrors()` and `EnableSensitiveDataLogging()` only when `IsDevelopment()`.
- The `IClock` abstraction returns `DateTimeOffset`, not `DateTime`. The whole codebase will use it.

## 9. Suggested execution outline

1. Add NuGet packages via CPM (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, `Testcontainers.PostgreSql`).
2. Create the abstractions in `Application/Common/Abstractions/`.
3. Create `SlotSmartDbContext`, `TenantStampingInterceptor` (no-op), `NoopTenantContext`, `SystemClock`.
4. Wire `services.AddInfrastructure(...)` and call from `Program.cs`.
5. Add `DesignTimeDbContextFactory`.
6. Generate `InitialCreate` migration.
7. Add Testcontainers base + smoke test.
8. Update README + CHANGELOG.

## 10. Open questions / risks

- Risk: developers on Apple Silicon may need explicit Postgres image tag. **Mitigation**: pin `postgres:16-alpine` in Testcontainers and document.
- Question: do we add the outbox tables now? **Decision**: no — wait until first domain event publishes (P5-T01 or later).
