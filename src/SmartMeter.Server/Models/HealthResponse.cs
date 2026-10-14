namespace SmartMeter.Server.Models;

public sealed record HealthResponse
{
    public required string Status { get; init; }
}
