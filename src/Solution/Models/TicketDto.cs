using System.Text.Json.Serialization;

namespace Solution.Models;

public record TicketDto(
    int Id,
    string Type,
    string CarId,
    DateTime RecordedAtUtc,
    int? SpeedKmh,
    string? Reason,
    string? ExternalTicketId);

public record TicketListResponse(IReadOnlyList<TicketDto> Tickets);
