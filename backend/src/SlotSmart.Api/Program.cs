using SlotSmart.Application;
using SlotSmart.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSlotSmartApplication()
    .AddSlotSmartInfrastructure();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Versioned health endpoint. The richer DB-ping version lands in P1-T06 once the DbContext exists.
app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health")
   .WithTags("System");

app.Run();

/// <summary>Marker so SlotSmart.Api.Tests can build a WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program;
