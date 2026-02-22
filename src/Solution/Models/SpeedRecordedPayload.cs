using System.Text.Json.Serialization;

namespace Solution.Models;

public record SpeedRecordedPayload(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("speed")] int Speed,
    [property: JsonPropertyName("cameraId")] string CameraId,
    [property: JsonPropertyName("timestamp")] string Timestamp) : WebhookEventBase(Event, Timestamp);

