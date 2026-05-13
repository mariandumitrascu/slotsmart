using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlotSmart.Infrastructure.Persistence;
using SlotSmart.Infrastructure.Persistence.Interceptors;

namespace SlotSmart.Infrastructure.Tests;

/// <summary>
/// End-to-end smoke for the EF Core stack: Postgres reachable, migrations applied,
/// schema present, SaveChanges path goes through the <see cref="TenantStampingInterceptor"/>.
/// </summary>
public sealed class DbContextSmokeTests : PostgresIntegrationTestBase, IClassFixture<PostgresContainerFixture>
{
    public DbContextSmokeTests(PostgresContainerFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Migrations_apply_and_app_schema_is_created()
    {
        await using var dbContext = Fixture.CreateDbContext();

        // Querying the EF migrations history table proves the migration ran.
        var historyTableExists = await TableExistsAsync(
            dbContext,
            schema: SlotSmartDbContext.DefaultSchema,
            tableName: "__EFMigrationsHistory");

        historyTableExists.Should().BeTrue(
            "the InitialCreate migration must create the EF history table inside the 'app' schema.");

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        appliedMigrations.Should().Contain(name => name.EndsWith("_InitialCreate", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SaveChangesAsync_invokes_the_TenantStampingInterceptor()
    {
        // Build a fresh DI container that uses the container's connection string AND the same
        // TenantStampingInterceptor instance — so we can assert that SaveChangesAsync dispatched through it.
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [DependencyInjection.PostgresConnectionStringKey] = Fixture.ConnectionString
            })
            .Build();

        services.AddSlotSmartInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var interceptor = scope.ServiceProvider.GetRequiredService<TenantStampingInterceptor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<SlotSmartDbContext>();

        // Make sure the schema is present in this scope's DbContext too.
        await dbContext.Database.MigrateAsync();

        var before = interceptor.InvocationCount;

        // No tracked changes — SaveChanges still goes through the interceptor pipeline,
        // which is exactly what we want to assert (the interceptor is wired into the options).
        await dbContext.SaveChangesAsync();

        interceptor.InvocationCount.Should().BeGreaterThan(before,
            "TenantStampingInterceptor must observe every SaveChanges call.");
    }

    private static async Task<bool> TableExistsAsync(SlotSmartDbContext dbContext, string schema, string tableName)
    {
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT 1 FROM information_schema.tables WHERE table_schema = @schema AND table_name = @table";
            var schemaParam = cmd.CreateParameter();
            schemaParam.ParameterName = "schema";
            schemaParam.Value = schema;
            cmd.Parameters.Add(schemaParam);
            var tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "table";
            tableParam.Value = tableName;
            cmd.Parameters.Add(tableParam);

            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
