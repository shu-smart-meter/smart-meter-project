namespace SmartMeter.Api.Models;

public sealed record HealthResponse
{
    public required string Status { get; init; }
}
