# Multi-Tenancy Strategy

SlotSmart is multi-tenant from day one. Each **tennis club** is one **tenant**. A user belongs to exactly one tenant (the MVP simplification; cross-tenant membership is deferred).

## 1. Isolation model

We use **shared database, shared schema, row-level tenant column**.

Pros: cheapest to operate, fastest to develop, simplest migrations.
Cons: a single tenant cannot have its own backup window; large tenants in the future may need to be promoted to dedicated DBs.

> A future "noisy tenant promotion" path is reserved (move a tenant to its own DB) but explicitly out of scope for MVP.

### Required column

Every multi-tenant table has:

```csharp
public Guid TenantId { get; private set; }
```

Stored as `uuid NOT NULL` with a non-nullable index. Most tables get a composite index `(TenantId, <hot column>)`.

### Always-on global query filter

`SlotSmartDbContext` applies an EF Core global query filter on every multi-tenant entity:

```csharp
modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
```

The filter MUST be enforced for **every** multi-tenant entity. An architecture test asserts this: every type implementing `IMultiTenantEntity` must have a matching query filter.

### Write-side enforcement

A SaveChanges interceptor (`TenantStampingInterceptor`) sets `TenantId` on inserted entities from `ITenantContext`. Updates that try to change `TenantId` throw.

## 2. Tenant resolution

Order of resolution (first wins):

1. **JWT claim** `tenant_id` — for authenticated calls.
2. **Subdomain** `<slug>.slotsmart.app` — for marketing/login flows; resolves to a tenant by `Slug`.
3. **Header** `X-Tenant-Slug` — for local dev and admin tooling.

If none match for a request that requires tenancy → `401 / 403` with `tenant_required` problem detail.

A small set of endpoints is **tenant-agnostic** (health, OpenAPI, public signup, password reset). These are explicitly opted out via attribute / endpoint metadata `[AllowAnonymousTenant]`.

## 3. Tenant context

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    bool IsResolved { get; }
}
```

Registered as **scoped**. Implementation is `HttpTenantContext` for the API; tests use `FakeTenantContext`. Background jobs set the tenant explicitly via `using (tenantScope.Begin(tenantId)) { … }`.

## 4. Tenant lifecycle

- **Provisioning**: a `Tenant` row is created during club signup (Phase 2 / Phase 3). Includes `Slug`, `Name`, `Status`, `PlanCode`.
- **Status**: `PendingActivation | Active | Suspended | Deleted`. Suspended tenants get 403 on all data endpoints.
- **Soft delete**: deletion is soft; data is retained for retention period (configurable). Hard delete is an admin job.

## 5. Cross-tenant admin

A platform-admin role exists (`PlatformAdmin`). Platform admins use a separate set of endpoints (`/admin/*`) where the tenant filter is **disabled** and the tenant is supplied explicitly per request.

## 6. Tests

- Every multi-tenant repository/handler test runs with at least two seeded tenants and asserts that operations in one tenant cannot see/modify data in another.
- An architecture test asserts: every `IMultiTenantEntity` implementer is configured with a query filter.

## 7. Decisions deferred

- Cross-tenant identity (one human being a coach at multiple clubs) — deferred to V2. MVP: one user belongs to exactly one tenant.
- Per-tenant data residency / region pinning — V2.
- Per-tenant custom domains beyond subdomains — V2.
