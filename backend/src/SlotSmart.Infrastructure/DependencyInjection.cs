using Microsoft.Extensions.DependencyInjection;

namespace SlotSmart.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Wires up the DbContext, identity, email,
/// jobs, outbox, etc. as those land in later tasks (P1-T02 onwards).
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registers Infrastructure-layer services. No-op until DbContext / repositories are added.</summary>
    public static IServiceCollection AddSlotSmartInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
