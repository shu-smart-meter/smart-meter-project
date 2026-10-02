namespace SmartMeter.Model;

// the challenge a meter must sign to prove it owns its key
public sealed record ChallengeResponse
{
    public required string ChallengeId { get; init; }

    // base64 nonce; the client signs these exact bytes
    public required string Nonce { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}