using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(c =>
    {
        c.AddJsonFile("appsettings.json", optional: true);
        c.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient("webhooks", (_, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
    })
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var urlsRaw = config["Webhooks:Urls"] ?? config["Webhooks__Urls"] ?? "";
var urls = urlsRaw
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Where(u => u.Length > 0)
    .ToList();

var validUrls = new List<string>();
foreach (var u in urls)
{
    if (!Uri.TryCreate(u, UriKind.Absolute, out var uri) || !uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[Producer] Skipping invalid URL: {0}", u);
        continue;
    }
    validUrls.Add(uri.ToString());
}

if (validUrls.Count == 0)
{
    Console.WriteLine("[Producer] No valid webhook URLs configured. Set Webhooks__Urls (e.g. comma-separated list). Exiting.");
    return 1;
}

var intervalSeconds = config.GetValue("Producer:IntervalSeconds", 2);
var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var httpFactory = host.Services.GetRequiredService<IHttpClientFactory>();
var index = 0;

Console.WriteLine("[Producer] Sending to {0} webhook(s), interval {1}s.", validUrls.Count, intervalSeconds);

while (true)
{
    var payload = EventGenerator.GenerateNext(index++);
    var json = JsonSerializer.Serialize(payload, payload.GetType(), jsonOptions);

    foreach (var url in validUrls)
    {
        try
        {
            using var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var client = httpFactory.CreateClient("webhooks");
            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine("[Producer] {0} -> {1} {2}", url, (int)response.StatusCode, response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Producer] {0} -> Error: {1}", url, ex.Message);
        }
    }

    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
}
