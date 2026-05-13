namespace SlotSmart.Domain.Common;

/// <summary>
/// Marker for any entity that belongs to a single tenant (club).
/// </summary>
/// <remarks>
/// <see cref="TenantId"/> is the surrogate <c>bigint</c> FK to <c>Tenants.Id</c>
/// (per ADR-007 — dual-key pattern). The Guid form of the tenant id lives in
/// <c>ITenantContext.TenantEntityId</c> (added in P2-T01) and on <c>Tenant.EntityId</c>;
/// it is never duplicated on per-row tables to save 16 bytes per row.
/// <para>
/// The EF Core global query filter compares this <c>long</c> against
/// <c>ITenantContext.TenantId</c>. The tenant-stamping interceptor populates it on insert.
/// </para>
/// </remarks>
public interface IMultiTenantEntity
{
    /// <summary>Surrogate FK to <c>Tenants.Id</c>. Stamped by the interceptor on insert.</summary>
    long TenantId { get; }
}
