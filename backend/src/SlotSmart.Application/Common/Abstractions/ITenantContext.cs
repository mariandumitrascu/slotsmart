namespace SlotSmart.Application.Common.Abstractions;

/// <summary>
/// Per-request / per-scope tenant identity. Resolved by middleware (HTTP) or set explicitly
/// in background-job scopes. Carries both the surrogate <c>bigint</c> id (used by the EF Core
/// global query filter and FKs) and the public <see cref="Guid"/> form (used in JWT claims,
/// audit entries, and public URLs) per ADR-007 / multi-tenancy-strategy.md §3.
/// </summary>
/// <remarks>
/// Registered with <c>AddScoped&lt;ITenantContext, …&gt;</c>. The default implementation in
/// P1-T02 is <c>NoopTenantContext</c> which always returns "unresolved" — every multi-tenant
/// query then matches nothing, which is the correct safe default until P2-T01 lands real
/// HTTP/JWT-driven resolution.
/// </remarks>
public interface ITenantContext
{
    /// <summary>Surrogate <c>bigint</c> FK to <c>Tenants.Id</c>. Compared by the query filter.</summary>
    long TenantId { get; }

    /// <summary>UUIDv7. Used in JWT claims (<c>tenant_id</c>), audit, public URLs.</summary>
    Guid TenantEntityId { get; }

    /// <summary>Tenant URL slug (e.g., "club-vienna"). Lowercase, kebab-case.</summary>
    string TenantSlug { get; }

    /// <summary>True iff a tenant was successfully resolved for this scope.</summary>
    bool IsResolved { get; }
}
