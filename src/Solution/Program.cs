var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Placeholder: accept webhook payloads. Candidate implements storage, deduplication, and ticket logic.
app.MapPost("/webhook", (HttpRequest _) => Results.Ok());

app.MapGet("/", () => Results.Ok("Solution service. Implement webhook handling per README.md."));

app.Run();
