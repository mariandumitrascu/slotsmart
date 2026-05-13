using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlotSmart.Infrastructure.Persistence.Configurations;

namespace SlotSmart.Architecture.Tests;

/// <summary>
/// Asserts that every concrete <see cref="IEntityTypeConfiguration{TEntity}"/> implementer in
/// <c>SlotSmart.Infrastructure</c> derives from <see cref="EntityConfiguration{TEntity}"/>.
/// </summary>
/// <remarks>
/// Until P3 there are no concrete entity configurations, so this test passes vacuously today.
/// As soon as the first <c>MemberConfiguration</c> / <c>ClubSettingsConfiguration</c> lands,
/// it must inherit from <see cref="EntityConfiguration{TEntity}"/> or this test fails — preventing
/// accidental config classes that bypass the dual-key surrogate-PK wiring (ADR-007).
/// </remarks>
public sealed class EntityConfigurationDerivationTests
{
    private static readonly Assembly InfrastructureAssembly = typeof(EntityConfiguration<>).Assembly;
    private static readonly Type EntityConfigurationOpen = typeof(EntityConfiguration<>);
    private static readonly Type EntityTypeConfigurationOpen = typeof(IEntityTypeConfiguration<>);

    [Fact]
    public void Every_IEntityTypeConfiguration_implementer_derives_from_EntityConfiguration()
    {
        var concreteConfigs = InfrastructureAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(ImplementsEntityTypeConfiguration)
            .ToList();

        var violators = concreteConfigs
            .Where(t => !DerivesFromEntityConfigurationBase(t))
            .Select(t => t.FullName)
            .ToList();

        violators.Should().BeEmpty(
            "Every concrete IEntityTypeConfiguration<> in SlotSmart.Infrastructure must derive " +
            "from EntityConfiguration<> so the surrogate-PK shadow property + EntityId index " +
            "are configured uniformly (ADR-007). Violators: " + string.Join(", ", violators));
    }

    private static bool ImplementsEntityTypeConfiguration(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == EntityTypeConfigurationOpen);

    private static bool DerivesFromEntityConfigurationBase(Type type)
    {
        var current = type.BaseType;
        while (current is not null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == EntityConfigurationOpen)
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}
