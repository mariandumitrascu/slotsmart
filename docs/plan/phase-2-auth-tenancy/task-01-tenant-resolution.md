# Task `P2-T01` — Tenant Resolution & EF Query Filters

> **Phase**: 2 — Auth & Multi-Tenancy
> **Estimated size**: M
> **Depends on**: Phase 1 complete
> **Can run in parallel with**: nothing significant in Phase 2

---

## 1. Context

[`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md) defines the shared-schema / row-level-tenant approach. We need a concrete, well-tested implementation: tenant resolution from the request, a scoped `ITenantContext`, the SaveChanges interceptor that stamps `TenantId`, and the EF Core query filter machinery that auto-applies the filter to every multi-tenant entity.

There are no business entities yet, but we add a **trivial test entity** that implements `IMultiTenantEntity` purely so we can write tests proving the filter works. We remove that entity (or move it to a test fixture) in Phase 3 when real aggregates land.

## 2. Goal

> Two tenants exist in the DB; with `ITenantContext` set to tenant A, queries against the test entity return only A's rows, and any insert through `SaveChangesAsync` is auto-stamped with A's `TenantId` — verified by integration tests.

## 3. Scope

### In scope

- Introduce `IMultiTenantEntity` marker interface in `Domain` (or `Shared`):
  ```csharp
  public interface IMultiTenantEntity { Guid TenantId { get; } }
  ```
- Implement `HttpTenantContext` in `Infrastructure/Tenancy/` reading the tenant from:
  1. `tenant_id` claim in `HttpContext.User`
  2. Subdomain (`<slug>.slotsmart.app`) — resolved via a `Tenant` table read
  3. `X-Tenant-Slug` header (dev-only; emit a warning log if used in non-development)
- Add a `Tenant` aggregate in `Domain` with: `Id`, `Slug`, `Name`, `Status (PendingActivation|Active|Suspended|Deleted)`, `TimeZoneId`, `CreatedAt`, `Version`.
- Add the `Tenant` table via a migration.
- Implement `TenantStampingInterceptor` that:
  - On `Added` for `IMultiTenantEntity`, sets `TenantId` from `ITenantContext.TenantId` if not already set.
  - Throws on `Modified` if `TenantId` changed.
- Implement `SlotSmartDbContext.OnModelCreating` to apply the global query filter `e.TenantId == _tenantContext.TenantId` to every `IMultiTenantEntity`. Use reflection to enumerate all such entities and apply consistently.
- Add `[AllowAnonymousTenant]` endpoint metadata (`IRequireNoTenantMetadata`) so we can mark public endpoints; middleware enforces "tenant resolved" for the rest.
- Architecture test: every `IMultiTenantEntity` implementer is configured with a query filter.
- Tests using **two seeded tenants** and a fake `IMultiTenantEntity` to prove isolation in reads, updates, deletes.

### Out of scope

- JWT claim extraction → P2-T02 will fill the JWT path; for this task it's enough to wire the **resolution** code and test the header-based path.
- Subdomain DNS / hosting → P2-T04 will document, but the code path here just reads `HttpContext.Request.Host`.
- Real aggregates → Phase 3+.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/multi-tenancy-strategy.md`](../00-architecture/multi-tenancy-strategy.md)
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Env vars: none new.

## 5. Deliverables

### Domain

- `backend/src/SlotSmart.Domain/Common/IMultiTenantEntity.cs`
- `backend/src/SlotSmart.Domain/Tenants/Tenant.cs` (aggregate)
- `backend/src/SlotSmart.Domain/Tenants/TenantStatus.cs`

### Application

- `backend/src/SlotSmart.Application/Common/Abstractions/ITenantContext.cs` (refined)
- `backend/src/SlotSmart.Application/Common/Tenancy/RequireTenantAttribute.cs` and the inverse `AllowAnonymousTenantAttribute.cs`.

### Infrastructure

- `backend/src/SlotSmart.Infrastructure/Tenancy/HttpTenantContext.cs`
- `backend/src/SlotSmart.Infrastructure/Tenancy/TenantResolutionMiddleware.cs`
- `backend/src/SlotSmart.Infrastructure/Persistence/Interceptors/TenantStampingInterceptor.cs` (real implementation)
- `backend/src/SlotSmart.Infrastructure/Persistence/Configurations/TenantConfiguration.cs`
- Migration: `AddTenant`.

### API

- `Program.cs` wires `TenantResolutionMiddleware` **after** authentication middleware (so the JWT claim path can win).

### Tests

- `backend/tests/SlotSmart.Infrastructure.Tests/Tenancy/TenantQueryFilterTests.cs`
- `backend/tests/SlotSmart.Infrastructure.Tests/Tenancy/TenantStampingInterceptorTests.cs`
- `backend/tests/SlotSmart.Architecture.Tests/MultiTenancyArchitectureTests.cs` — every `IMultiTenantEntity` implementer is configured with a query filter via reflection on the model.

### Docs

- Update `CHANGELOG.md`.

## 6. Acceptance Criteria

- [ ] With two seeded tenants A and B and seeded test entities in each, a request with `X-Tenant-Slug: <A.slug>` sees only A's rows.
- [ ] An insert via the DbContext under tenant A is automatically stamped with A's `TenantId`.
- [ ] Trying to change `TenantId` on an existing row throws on `SaveChangesAsync`.
- [ ] A request to a tenant-required endpoint without a resolvable tenant returns `401` with problem+json `type=slotsmart/errors/tenant-required`.
- [ ] Public endpoints (health, openapi, docs, and any with `[AllowAnonymousTenant]`) work without a tenant header.
- [ ] Architecture test fails if a future PR adds a new `IMultiTenantEntity` without a query filter.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] Integration tests use two tenants and isolation is verified for queries, updates, and deletes.
- [ ] CHANGELOG.md updated.
- [ ] No `TenantId` leaks: an architecture test ensures aggregates set `TenantId` only through the interceptor / constructor, not via public setters.

## 8. Handoff notes / gotchas

- `ITenantContext` must be **scoped**. Resolve it inside the DbContext via a constructor-injected `Func<ITenantContext>` factory; this avoids the captured-value-of-startup-tenant trap when the context is pooled.
- The global query filter must reference the context's tenant **at query execution time**, not at model build time. Achieve this by using `(this as IDbContextWithTenant).TenantId`-style access or by holding a small `_tenantContext` field and reading via a closure.
- Be careful with `DbContextPool` + query filters — the filter is compiled once. Using the closure approach above is supported by EF Core, but verify with a test that switches tenants on the same context.
- The migration for `Tenant` creates the table only; no seeded rows. Seeding happens in tests via builders.
- Subdomain resolution must trim ports and ignore `localhost`; the header path is the dev fallback.

## 9. Suggested execution outline

1. Add `IMultiTenantEntity` to Domain.
2. Define `Tenant` aggregate + configuration + migration.
3. Refactor the existing no-op `TenantStampingInterceptor` into the real one; add tests.
4. Implement `HttpTenantContext` and `TenantResolutionMiddleware`.
5. Implement the global query filter machinery in `SlotSmartDbContext.OnModelCreating`.
6. Add the architecture test.
7. Add integration tests with two tenants.
8. Update CHANGELOG.

## 10. Open questions / risks

- Risk: forgetting to opt a new entity into the query filter is a silent disaster. **Mitigation**: the architecture test catches it.
- Question: how do platform admins bypass the filter? **Decision (deferred to V2)**: an `IDisposable AsPlatformAdminScope()` that flips a flag on the tenant context; out of scope here, but design `ITenantContext` so this can be added without breaking changes.
