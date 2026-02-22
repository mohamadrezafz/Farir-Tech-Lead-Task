using System.Text.Json.Serialization;

namespace Solution.Models;

public abstract record WebhookEventBase(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("timestamp")] string Timestamp);
