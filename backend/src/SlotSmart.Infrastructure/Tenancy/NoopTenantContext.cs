using SlotSmart.Application.Common.Abstractions;

namespace SlotSmart.Infrastructure.Tenancy;

/// <summary>
/// Default <see cref="ITenantContext"/> for P1-T02: always reports "unresolved".
/// </summary>
/// <remarks>
/// Real HTTP/JWT-driven resolution lands in P2-T01 (<c>HttpTenantContext</c>) and replaces
/// this registration. While this stub is in place, the EF Core global query filter compares
/// <c>e.TenantId == 0</c>, which matches no real row — the safe default. Multi-tenant inserts
/// would also be stamped with <c>TenantId = 0</c>, so the API has no business endpoints that
/// can write to a multi-tenant table until P2-T01 is in place.
/// </remarks>
public sealed class NoopTenantContext : ITenantContext
{
    public long TenantId => 0L;
    public Guid TenantEntityId => Guid.Empty;
    public string TenantSlug => string.Empty;
    public bool IsResolved => false;
}
