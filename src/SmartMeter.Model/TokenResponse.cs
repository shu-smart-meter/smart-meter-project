namespace SmartMeter.Model;

public sealed record TokenResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string MeterId { get; init; }
}
