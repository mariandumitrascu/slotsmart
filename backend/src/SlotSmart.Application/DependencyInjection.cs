using Microsoft.Extensions.DependencyInjection;

namespace SlotSmart.Application;

/// <summary>
/// Composition root for the Application layer. Wires up MediatR / FluentValidation /
/// pipeline behaviours as those land in later tasks.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registers Application-layer services. No-op until handlers are added.</summary>
    public static IServiceCollection AddSlotSmartApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Intentionally empty in P1-T01. Handlers, validators, and pipeline behaviours are wired in
        // their owning tasks (P2 onwards). Method exists so SlotSmart.Api can call it without churn.
        return services;
    }
}
