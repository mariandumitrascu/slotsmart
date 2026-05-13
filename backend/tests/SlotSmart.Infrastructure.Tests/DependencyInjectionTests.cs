using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlotSmart.Application.Common.Abstractions;
using SlotSmart.Infrastructure.Persistence;
using SlotSmart.Infrastructure.Persistence.Interceptors;

namespace SlotSmart.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddSlotSmartInfrastructure_throws_when_connection_string_is_missing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var act = () => services.AddSlotSmartInfrastructure(configuration);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Postgres connection string not configured*");
    }

    [Fact]
    public void AddSlotSmartInfrastructure_registers_required_services_with_a_valid_connection_string()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [DependencyInjection.PostgresConnectionStringKey] =
                    "Host=localhost;Port=5432;Database=slotsmart;Username=slotsmart;Password=slotsmart"
            })
            .Build();

        services.AddSlotSmartInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<ITenantContext>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IClock>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IUnitOfWork>().Should().NotBeNull();
        scope.ServiceProvider.GetService<TenantStampingInterceptor>().Should().NotBeNull();
        scope.ServiceProvider.GetService<SlotSmartDbContext>().Should().NotBeNull();
    }
}
