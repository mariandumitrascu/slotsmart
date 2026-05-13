using Microsoft.Extensions.DependencyInjection;

namespace SlotSmart.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddSlotSmartApplication_registers_without_throwing()
    {
        var services = new ServiceCollection();

        var act = () => services.AddSlotSmartApplication();

        act.Should().NotThrow();
    }
}
