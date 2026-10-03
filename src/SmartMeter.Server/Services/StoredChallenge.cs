using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// a challenge the server issued and will accept once, before it expires
public sealed record StoredChallenge
{
    // meter the challenge is bound to
    public required string MeterId { get; init; }

    // canonical public key bytes the proof must match
    public required byte[] PublicKey { get; init; }

    // raw nonce bytes the proof signature must cover
    public required byte[] Nonce { get; init; }

    // true when the key has not completed enrolment yet
    public required bool PendingEnrolment { get; init; }
}