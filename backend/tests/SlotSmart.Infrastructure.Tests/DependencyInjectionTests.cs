using Microsoft.Extensions.DependencyInjection;

namespace SlotSmart.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddSlotSmartInfrastructure_registers_without_throwing()
    {
        var services = new ServiceCollection();

        var act = () => services.AddSlotSmartInfrastructure();

        act.Should().NotThrow();
    }
}
