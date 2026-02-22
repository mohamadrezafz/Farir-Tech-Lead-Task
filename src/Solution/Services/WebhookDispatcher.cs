using System.Text.Json;
using Solution.Models;
using Solution.Services.Interfaces;

namespace Solution.Services;

public class WebhookDispatcher : IWebhookDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebhookHandlerService _handler;

    public WebhookDispatcher(IWebhookHandlerService handler)
    {
        _handler = handler;
    }

    public async Task<WebhookDispatchResult> DispatchAsync(string jsonBody, CancellationToken ct = default)
    {
        using var doc = JsonDocument.Parse(jsonBody);
        if (!doc.RootElement.TryGetProperty("event", out var eventEl))
            return WebhookDispatchResult.BadRequest("Missing 'event' field.");
        var eventType = eventEl.GetString();
        if (string.IsNullOrEmpty(eventType))
            return WebhookDispatchResult.BadRequest("Invalid event type.");

        try
        {
            return eventType switch
            {
                "CarGenerated" => await DispatchCarGeneratedAsync(jsonBody, ct),
                "SpeedRecorded" => await DispatchSpeedRecordedAsync(jsonBody, ct),
                "ManualTicket" => await DispatchManualTicketAsync(jsonBody, ct),
                _ => WebhookDispatchResult.BadRequest($"Unknown event type: {eventType}")
            };
        }
        catch (JsonException ex)
        {
            return WebhookDispatchResult.BadRequest($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return WebhookDispatchResult.ServerError(ex.Message);
        }
    }

    private async Task<WebhookDispatchResult> DispatchCarGeneratedAsync(string jsonBody, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<CarGeneratedPayload>(jsonBody, JsonOptions);
        if (payload is null)
            return WebhookDispatchResult.BadRequest("Invalid CarGenerated payload.");
        await _handler.ProcessCarGeneratedAsync(payload, ct);
        return WebhookDispatchResult.Ok();
    }

    private async Task<WebhookDispatchResult> DispatchSpeedRecordedAsync(string jsonBody, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<SpeedRecordedPayload>(jsonBody, JsonOptions);
        if (payload is null)
            return WebhookDispatchResult.BadRequest("Invalid SpeedRecorded payload.");
        await _handler.ProcessSpeedRecordedAsync(payload, ct);
        return WebhookDispatchResult.Ok();
    }

    private async Task<WebhookDispatchResult> DispatchManualTicketAsync(string jsonBody, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<ManualTicketPayload>(jsonBody, JsonOptions);
        if (payload is null)
            return WebhookDispatchResult.BadRequest("Invalid ManualTicket payload.");
        await _handler.ProcessManualTicketAsync(payload, ct);
        return WebhookDispatchResult.Ok();
    }
}
