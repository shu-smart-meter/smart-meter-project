namespace SmartMeter.Model;

// the result of a successful proof: a bearer token for later requests
public sealed record TokenResponse
{
    // opaque access token; send it as "Authorization: Bearer <token>"
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    // stable identity derived from the key, not from a database
    public required string MeterId { get; init; }
}