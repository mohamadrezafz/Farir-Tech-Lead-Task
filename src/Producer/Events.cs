using System.Text.Json.Serialization;

namespace Producer;

internal static class EventGenerator
{
    private static readonly string[] CarIds =
    {
        "car-01", "car-02", "car-03", "car-04", "car-05", "car-06", "car-07", "car-08", "car-09", "car-10",
        "car-11", "car-12", "car-13", "car-14", "car-15", "car-16", "car-17", "car-18", "car-19", "car-20"
    };

    private static readonly string[] Plates =
    {
        "AB 123", "CD 456", "EF 789", "GH 012", "IJ 345", "KL 678", "MN 901", "OP 234", "QR 567", "ST 890",
        "UV 111", "WX 222", "YZ 333", "AA 444", "BB 555", "CC 666", "DD 777", "EE 888", "FF 999", "GG 000"
    };

    private static readonly string[] CameraIds = { "cam-A", "cam-B", "cam-C" };
    private static readonly string[] Reasons = { "Speeding", "Running red", "Other violation" };
    private static readonly Random Rnd = new();

    public static object GenerateCarGenerated()
    {
        var i = Rnd.Next(CarIds.Length);
        return new CarGeneratedEvent(CarIds[i], Plates[i], DateTime.UtcNow.ToString("O"));
    }

    public static object GenerateSpeedRecorded()
    {
        var i = Rnd.Next(CarIds.Length);
        return new SpeedRecordedEvent(CarIds[i], Rnd.Next(70, 131), CameraIds[Rnd.Next(CameraIds.Length)], DateTime.UtcNow.ToString("O"));
    }

    public static object GenerateManualTicket()
    {
        var i = Rnd.Next(CarIds.Length);
        return new ManualTicketEvent($"ticket-{Guid.NewGuid():N}"[..20], CarIds[i], Reasons[Rnd.Next(Reasons.Length)], DateTime.UtcNow.ToString("O"));
    }

    public static object GenerateNext(int roundRobinIndex)
    {
        return (roundRobinIndex % 3) switch
        {
            0 => GenerateCarGenerated(),
            1 => GenerateSpeedRecorded(),
            _ => GenerateManualTicket()
        };
    }
}

internal record CarGeneratedEvent(
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("plateNumber")] string PlateNumber,
    [property: JsonPropertyName("timestamp")] string Timestamp)
{
    [JsonPropertyName("event")]
    public string Event => "CarGenerated";
}

internal record SpeedRecordedEvent(
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("speed")] int Speed,
    [property: JsonPropertyName("cameraId")] string CameraId,
    [property: JsonPropertyName("timestamp")] string Timestamp)
{
    [JsonPropertyName("event")]
    public string Event => "SpeedRecorded";
}

internal record ManualTicketEvent(
    [property: JsonPropertyName("ticketId")] string TicketId,
    [property: JsonPropertyName("carId")] string CarId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("timestamp")] string Timestamp)
{
    [JsonPropertyName("event")]
    public string Event => "ManualTicket";
}
