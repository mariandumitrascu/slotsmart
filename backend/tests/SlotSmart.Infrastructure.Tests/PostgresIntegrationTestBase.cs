using Microsoft.EntityFrameworkCore;
using SlotSmart.Infrastructure.Persistence;
using SlotSmart.Infrastructure.Tenancy;
using Testcontainers.PostgreSql;

namespace SlotSmart.Infrastructure.Tests;

/// <summary>
/// xUnit class fixture that boots a disposable Postgres container per test class via Testcontainers,
/// applies the SlotSmart EF Core migrations once, and exposes a connection string + DbContext factory.
/// </summary>
/// <remarks>
/// Costs: ~1–3 seconds per fixture startup (the postgres:16-alpine image is ~85 MB and is cached
/// after the first pull). Within a fixture all tests share the same Postgres instance.
/// Apple-Silicon note: <c>postgres:16-alpine</c> ships native arm64 — no platform override needed.
/// </remarks>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("slotsmart_test")
        .WithUsername("slotsmart_test")
        .WithPassword("slotsmart_test")
        .Build();

    /// <summary>The connection string for this container. Available after <see cref="InitializeAsync"/>.</summary>
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Apply migrations once per fixture so individual tests don't pay the cost.
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Creates a fresh <see cref="SlotSmartDbContext"/> against the container.</summary>
    public SlotSmartDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SlotSmartDbContext>()
            .UseNpgsql(ConnectionString, npg =>
            {
                npg.MigrationsAssembly("SlotSmart.Infrastructure");
                npg.MigrationsHistoryTable("__EFMigrationsHistory", SlotSmartDbContext.DefaultSchema);
            })
            .Options;

        return new SlotSmartDbContext(options, new NoopTenantContext());
    }
}

/// <summary>
/// Base class for integration tests that need a real Postgres. Inherit and add an
/// <see cref="IClassFixture{TFixture}"/> binding to <see cref="PostgresContainerFixture"/>.
/// </summary>
public abstract class PostgresIntegrationTestBase
{
    protected PostgresContainerFixture Fixture { get; }

    protected PostgresIntegrationTestBase(PostgresContainerFixture fixture)
    {
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }
}
