using Microsoft.EntityFrameworkCore;
using SlotSmart.Application;
using SlotSmart.Infrastructure;
using SlotSmart.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSlotSmartApplication();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSlotSmartInfrastructureForDevelopment(builder.Configuration);
}
else
{
    builder.Services.AddSlotSmartInfrastructure(builder.Configuration);
}

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await ApplyMigrationsInDevelopmentAsync(app);
}

// Versioned health endpoint. The richer DB-ping version lands in P1-T06.
app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health")
   .WithTags("System");

app.Run();

// Migration application is dev-only. Production deploys run migrations out-of-band (P1-T04 / CD).
// We log on failure but do NOT crash the host so that misconfiguration is obvious without
// taking down the API entirely (e.g., devs forget to start their local Postgres).
static async Task ApplyMigrationsInDevelopmentAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup.Migrations");

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SlotSmartDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Applied EF Core migrations on startup (Development).");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex,
            "Failed to apply migrations on startup. The API will run but database calls will fail. " +
            "Verify Postgres is reachable at the configured connection string.");
    }
}

/// <summary>Marker so SlotSmart.Api.Tests can build a WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program;
