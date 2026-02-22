namespace Solution.Data.Entities;

public class SpeedReading
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public int SpeedKmh { get; set; }
    public string CameraId { get; set; } = null!;
    public DateTime RecordedAtUtc { get; set; }

    public Ticket? SpeedTicket { get; set; }
}
