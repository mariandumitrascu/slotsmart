using Microsoft.EntityFrameworkCore.Diagnostics;
using SlotSmart.Application.Common.Abstractions;

namespace SlotSmart.Infrastructure.Persistence.Interceptors;

/// <summary>
/// SaveChanges interceptor that will stamp <c>TenantId</c> on inserted multi-tenant entities
/// and reject updates that mutate <c>TenantId</c> on existing rows.
/// </summary>
/// <remarks>
/// In P1-T02 this is a **wired but functionally empty stub** — it counts invocations so
/// integration tests can assert the interceptor is on the SaveChanges path, but it does not
/// inspect or mutate any entries. The real stamping logic lands in P2-T01 along with the
/// first multi-tenant entity and a non-noop <see cref="ITenantContext"/>.
/// </remarks>
public sealed class TenantStampingInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private long _invocationCount;

    public TenantStampingInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <summary>How many times this interceptor's SavingChanges hook has fired in the current process.</summary>
    /// <remarks>Visible for tests that assert the interceptor is wired into the DbContext options.</remarks>
    public long InvocationCount => Interlocked.Read(ref _invocationCount);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _invocationCount);

        // Real stamping logic (set TenantId on Added IMultiTenantEntity, throw on Modified mismatch)
        // lands in P2-T01 once IMultiTenantEntity has its first concrete implementer AND
        // ITenantContext.IsResolved is wired to actual HTTP / JWT resolution.
        _ = _tenantContext;

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
