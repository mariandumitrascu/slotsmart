using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SlotSmart.Infrastructure.Tenancy;

namespace SlotSmart.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> tooling construct <see cref="SlotSmartDbContext"/> without booting the
/// full host. Used by <c>dotnet ef migrations add</c>, <c>dotnet ef database update</c>, etc.
/// </summary>
/// <remarks>
/// Connection string resolution order (first wins):
/// <list type="number">
///   <item>Env var <c>Postgres__ConnectionString</c> (matches the runtime config key).</item>
///   <item>Hardcoded local fallback — `localhost:5432`, user `slotsmart`, password `slotsmart`.
///         Local-only credentials, never used outside developer machines.</item>
/// </list>
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SlotSmartDbContext>
{
    public SlotSmartDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("Postgres__ConnectionString")
            ?? "Host=localhost;Port=5432;Database=slotsmart;Username=slotsmart;Password=slotsmart;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<SlotSmartDbContext>()
            .UseNpgsql(connectionString, npg =>
            {
                npg.MigrationsAssembly("SlotSmart.Infrastructure");
                npg.MigrationsHistoryTable("__EFMigrationsHistory", SlotSmartDbContext.DefaultSchema);
            })
            .Options;

        return new SlotSmartDbContext(options, new NoopTenantContext());
    }
}
