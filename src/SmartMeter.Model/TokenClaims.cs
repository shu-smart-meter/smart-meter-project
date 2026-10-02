namespace SmartMeter.Model;

// identity claims proven by a valid access token; the only view
// controllers and the hub get of the authentication exchange
public sealed record TokenClaims
{
    public required string MeterId { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}