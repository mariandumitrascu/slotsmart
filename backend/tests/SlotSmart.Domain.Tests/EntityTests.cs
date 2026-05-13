using SlotSmart.Domain.Common;
using SlotSmart.Shared.Identifiers;

namespace SlotSmart.Domain.Tests;

public sealed class EntityTests
{
    private sealed class FakeEntity : Entity
    {
        public FakeEntity(Guid id) => EntityId = id;
    }

    private sealed class OtherFakeEntity : Entity
    {
        public OtherFakeEntity(Guid id) => EntityId = id;
    }

    [Fact]
    public void Two_entities_of_the_same_type_with_the_same_EntityId_are_equal()
    {
        var id = UuidV7.NewGuid();
        var a = new FakeEntity(id);
        var b = new FakeEntity(id);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_entities_of_different_types_with_the_same_EntityId_are_not_equal()
    {
        var id = UuidV7.NewGuid();
        Entity a = new FakeEntity(id);
        Entity b = new OtherFakeEntity(id);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Two_entities_with_different_EntityIds_are_not_equal()
    {
        var a = new FakeEntity(UuidV7.NewGuid());
        var b = new FakeEntity(UuidV7.NewGuid());

        a.Should().NotBe(b);
    }
}
