namespace Solution.Data.Entities;

public class Car
{
    public int Id { get; set; }
    public string CarId { get; set; } = null!;  // Producer's car id, unique
    public string PlateNumber { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SpeedReading> SpeedReadings { get; set; } = new List<SpeedReading>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
