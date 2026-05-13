using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlotSmart.Application.Common.Abstractions;

namespace SlotSmart.Infrastructure.Persistence;

/// <summary>
/// Single EF Core <see cref="DbContext"/> for the SlotSmart application database.
/// </summary>
/// <remarks>
/// Conventions applied here (per ADR-007 / entity-identity.md and multi-tenancy-strategy.md):
/// <list type="bullet">
///   <item>Entity configurations are auto-discovered from this assembly via
///         <c>ApplyConfigurationsFromAssembly</c>. Every concrete configuration MUST inherit from
///         <c>EntityConfiguration&lt;T&gt;</c> (asserted by an architecture test).</item>
///   <item>Multi-tenant query filters are applied in <see cref="OnModelCreating"/> for every
///         entity implementing <c>IMultiTenantEntity</c>; until P2-T01 the noop tenant context
///         returns <c>TenantId == 0</c>, which matches no real row.</item>
///   <item>The <c>TenantStampingInterceptor</c> is added at registration time
///         (see <c>SlotSmart.Infrastructure.DependencyInjection</c>).</item>
/// </list>
/// </remarks>
public sealed class SlotSmartDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public SlotSmartDbContext(DbContextOptions<SlotSmartDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <summary>Database schema for application tables. Identity tables (P2-T02) will live in <c>identity</c>.</summary>
    public const string DefaultSchema = "app";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(DefaultSchema);

        // Apply every IEntityTypeConfiguration<T> in this assembly. Concrete configs land in
        // their owning phase (P3 onwards). The configuration base class enforces the dual-key pattern.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Tenant query filters for IMultiTenantEntity implementers go here once concrete entities exist.
        // The plumbing reads from _tenantContext so every filter compares against the live surrogate id.
        // No-op today (no concrete entities yet); architecture test in P2-T01 will keep this honest.
        _ = _tenantContext;

        base.OnModelCreating(modelBuilder);
    }
}
