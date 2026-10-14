namespace SmartMeter.Model;

public sealed record MeterTokenRequest
{
    public required string MeterId { get; init; }

    public required string ClientSecret { get; init; }
}
