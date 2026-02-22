using System.Text.Json.Serialization;

namespace Solution.Models;
public record CarGeneratedPayload(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("plateNumber")] string PlateNumber,
    [property: JsonPropertyName("timestamp")] string Timestamp) : WebhookEventBase(Event, Timestamp);
