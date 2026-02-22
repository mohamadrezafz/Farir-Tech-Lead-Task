namespace Solution.Data.Entities;

public enum TicketType { Speed, Manual }

public class Ticket
{
    public int Id { get; set; }
    public TicketType Type { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public DateTime RecordedAtUtc { get; set; }

    // Speed ticket: link to reading
    public int? SpeedReadingId { get; set; }
    public SpeedReading? SpeedReading { get; set; }

    // Manual ticket: from Producer
    public string? ExternalTicketId { get; set; }
    public string? Reason { get; set; }
}
