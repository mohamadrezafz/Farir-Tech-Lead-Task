using System.Text.Json.Serialization;

namespace Solution.Models;

public record SpeedStatsResponse(
    [property: JsonPropertyName("avg")] double Avg,
    [property: JsonPropertyName("max")] int Max,
    [property: JsonPropertyName("min")] int Min) ;
