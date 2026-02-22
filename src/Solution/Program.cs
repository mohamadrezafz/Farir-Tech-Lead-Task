using Microsoft.EntityFrameworkCore;
using Solution.Data;
using Solution.Models;
using Solution.Services;
using Solution.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// SQLite; use env or config for path so Docker can use a volume if needed
var conn = builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection")
    ?? "Data Source=solution.db";
builder.Services.AddDbContextFactory<SolutionDbContext>(options =>
    options.UseSqlite(conn));

builder.Services.AddScoped<IWebhookHandlerService, WebhookHandlerService>();
builder.Services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
builder.Services.AddScoped<ITicketQueryService, TicketQueryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Solution API",
        Version = "v1",
        Description = "Traffic enforcement pipeline: webhook and query APIs."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Solution API v1"));

// Ensure DB exists and schema is created
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SolutionDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

app.MapPost("/webhook", async (HttpRequest req, IWebhookDispatcher dispatcher, CancellationToken ct) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync(ct);
    var result = await dispatcher.DispatchAsync(body, ct);
    return result.Success
        ? Results.Ok()
        : result.StatusCode == 400
            ? Results.BadRequest(result.ErrorMessage)
            : Results.Problem(detail: result.ErrorMessage, statusCode: result.StatusCode);
});

// GET /api/tickets?from=...&to=... (optional ISO8601 dates)
app.MapGet("/api/tickets", async (
    DateTime? from,
    DateTime? to,
    ITicketQueryService query,
    CancellationToken ct) =>
{
    var fromUtc = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : (DateTime?)null;
    var toUtc = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : (DateTime?)null;
    var result = await query.GetTicketsAsync(fromUtc, toUtc, ct);
    return Results.Ok(result);
});

// GET /api/cars/{carId}/speed-stats?minutes=30
app.MapGet("/api/cars/{carId}/speed-stats", async (
    string carId,
    int? minutes,
    ITicketQueryService query,
    CancellationToken ct) =>
{
    var windowMinutes = minutes ?? 30;
    if (windowMinutes <= 0 || windowMinutes > 60 * 24 * 7)
        return Results.BadRequest("minutes must be between 1 and 10080 (1 week).");
    var result = await query.GetCarSpeedStatsAsync(carId, windowMinutes, ct);
    if (result is null)
        return Results.NotFound();
    return Results.Ok(result);
});

app.MapGet("/", () => Results.Ok("Solution service. Webhook: POST /webhook. APIs: GET /api/tickets, GET /api/cars/{carId}/speed-stats"));

app.Run();
