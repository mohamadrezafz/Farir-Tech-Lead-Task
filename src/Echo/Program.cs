var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapMethods("/webhook", new[] { "GET", "POST", "PUT", "PATCH" }, async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();
    await Console.Out.WriteLineAsync($"{DateTime.UtcNow:O} {body}");
    return Results.Ok();
});

app.MapGet("/", () => Results.Ok("Echo service. POST to /webhook to see requests echoed to console."));

app.Run();
