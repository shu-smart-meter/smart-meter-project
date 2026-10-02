namespace SmartMeter.Model;

// a signed challenge, sent back to prove key possession
public sealed record ProofRequest
{
    public required string ChallengeId { get; init; }

    // base64 of the fixed 64-byte P1363 ECDSA signature over the nonce
    public required string Signature { get; init; }
}