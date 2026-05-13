using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlotSmart.Application.Common.Abstractions;
using SlotSmart.Infrastructure.Persistence;
using SlotSmart.Infrastructure.Persistence.Interceptors;
using SlotSmart.Infrastructure.Tenancy;
using SlotSmart.Infrastructure.Time;

namespace SlotSmart.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key under which the Postgres connection string lives.</summary>
    public const string PostgresConnectionStringKey = "Postgres:ConnectionString";

    private static string ResolveConnectionString(IConfiguration configuration) =>
        configuration[PostgresConnectionStringKey]
        ?? throw new InvalidOperationException(
            $"Postgres connection string not configured. Set '{PostgresConnectionStringKey}' " +
            "in configuration or the env var 'Postgres__ConnectionString'.");

    /// <summary>
    /// Registers Infrastructure-layer services: <see cref="SlotSmartDbContext"/> on Postgres,
    /// the <see cref="TenantStampingInterceptor"/>, the noop tenant context (until P2-T01),
    /// and <see cref="SystemClock"/>.
    /// </summary>
    public static IServiceCollection AddSlotSmartInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = ResolveConnectionString(configuration);

        // Tenant context — noop until P2-T01 replaces this with HttpTenantContext.
        services.AddScoped<ITenantContext, NoopTenantContext>();

        // Time — singleton; deterministic in tests.
        services.AddSingleton<IClock, SystemClock>();

        // Interceptor — scoped so it sees the per-request tenant context.
        services.AddScoped<TenantStampingInterceptor>();

        services.AddDbContext<SlotSmartDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npg =>
            {
                npg.MigrationsAssembly("SlotSmart.Infrastructure");
                npg.MigrationsHistoryTable("__EFMigrationsHistory", SlotSmartDbContext.DefaultSchema);
            });

            // Adds the interceptor to the SaveChanges pipeline. The interceptor itself is
            // resolved from the same scope as the DbContext (so per-request tenant context flows in).
            options.AddInterceptors(sp.GetRequiredService<TenantStampingInterceptor>());

            // Detailed errors / sensitive logging are dev-only — gated by the host below
            // when AddSlotSmartInfrastructureForDevelopment is called instead.
        });

        // IUnitOfWork is implemented by the DbContext (forwarder lives in this assembly).
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Same as <see cref="AddSlotSmartInfrastructure"/> but enables EF Core's detailed errors
    /// and sensitive-data logging. Call ONLY from <c>app.Environment.IsDevelopment()</c> branches.
    /// </summary>
    public static IServiceCollection AddSlotSmartInfrastructureForDevelopment(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = ResolveConnectionString(configuration);

        services.AddScoped<ITenantContext, NoopTenantContext>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<TenantStampingInterceptor>();

        services.AddDbContext<SlotSmartDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npg =>
            {
                npg.MigrationsAssembly("SlotSmart.Infrastructure");
                npg.MigrationsHistoryTable("__EFMigrationsHistory", SlotSmartDbContext.DefaultSchema);
            });

            options.AddInterceptors(sp.GetRequiredService<TenantStampingInterceptor>());
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        return services;
    }
}

/// <summary>Adapts <see cref="SlotSmartDbContext"/> to <see cref="IUnitOfWork"/>.</summary>
internal sealed class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly SlotSmartDbContext _dbContext;

    public EfCoreUnitOfWork(SlotSmartDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
