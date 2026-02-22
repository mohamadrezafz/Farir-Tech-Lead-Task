using System.Text.Json.Serialization;

namespace Solution.Models;

public record ManualTicketPayload(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("ticketId")] string TicketId,
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("timestamp")] string Timestamp) : WebhookEventBase(Event, Timestamp);
