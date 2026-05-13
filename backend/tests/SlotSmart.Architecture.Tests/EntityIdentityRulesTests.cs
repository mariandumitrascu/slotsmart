using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using SlotSmart.Domain.Common;

namespace SlotSmart.Architecture.Tests;

/// <summary>
/// Enforces the dual-key identity pattern (ADR-007 / docs/plan/00-architecture/entity-identity.md)
/// at compile/test time.
///
/// In P1-T01 there are no concrete entities yet; these tests are written so that they pass when
/// no concrete entity exists AND fail loudly the moment a concrete entity is added that violates
/// the rule. The "live" red-green is demonstrated in P1-T02 when the first aggregates land.
/// </summary>
public sealed class EntityIdentityRulesTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;

    [Fact]
    public void Every_concrete_persistent_entity_inherits_from_Entity()
    {
        // Find every concrete class in Domain that "looks like" a persistent entity:
        // implements IAggregateRoot OR IMultiTenantEntity. They MUST inherit from Entity.
        var concreteEntities = DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t)
                        || typeof(IMultiTenantEntity).IsAssignableFrom(t))
            .ToList();

        var violators = concreteEntities
            .Where(t => !typeof(Entity).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        violators.Should().BeEmpty(
            "Every aggregate / multi-tenant entity must inherit from Entity (ADR-007). " +
            "Violators: " + string.Join(", ", violators));
    }

    [Fact]
    public void EntityId_property_has_no_public_setter()
    {
        // Reflective check on Entity itself; once concrete entities exist, this still holds because
        // EntityId is declared on the base class as { get; protected init; }.
        var prop = typeof(Entity).GetProperty(nameof(Entity.EntityId));
        prop.Should().NotBeNull();

        var setter = prop!.GetSetMethod(nonPublic: false);
        setter.Should().BeNull("EntityId must not have a public setter (ADR-007 §3 / §11).");
    }

    [Fact]
    public void IMultiTenantEntity_TenantId_is_a_long_not_a_Guid()
    {
        // ADR-007 §4: TenantId is the surrogate bigint, not a Guid.
        var tenantIdProp = typeof(IMultiTenantEntity).GetProperty(nameof(IMultiTenantEntity.TenantId));
        tenantIdProp.Should().NotBeNull();
        tenantIdProp!.PropertyType.Should().Be<long>(
            "ADR-007 / multi-tenancy-strategy.md require TenantId to be the bigint surrogate.");
    }

    [Fact]
    public void Domain_classes_do_not_carry_KeyAttribute_on_EntityId()
    {
        // EF Core convention attributes belong in Infrastructure, not Domain. Specifically,
        // [Key] on EntityId would silently make the Guid the primary key, defeating ADR-007.
        var keyAttribute = typeof(System.ComponentModel.DataAnnotations.KeyAttribute);

        var result = Types
            .InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .Should()
            .NotHaveCustomAttribute(keyAttribute)
            .GetResult();

        result.IsSuccessful
            .Should().BeTrue("EntityId must not carry [Key]; the storage PK is the bigint surrogate (ADR-007).");
    }
}
