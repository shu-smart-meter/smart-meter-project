namespace SmartMeter.Model;

public sealed record TokenClaims
{
    public required string MeterId { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
