namespace SmartMeter.Model;

// what a meter sends to ask for an authentication challenge
public sealed record ChallengeRequest
{
    // base64 of the X.509 SubjectPublicKeyInfo of the meter key
    public required string PublicKey { get; init; }

    // shared enrolment credential; only checked when the key is unknown
    public string? EnrolmentSecret { get; init; }
}