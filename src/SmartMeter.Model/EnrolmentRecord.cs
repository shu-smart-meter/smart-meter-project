namespace SmartMeter.Model;

// a meter whose public key the server has accepted
public sealed record EnrolmentRecord
{
    public required string MeterId { get; init; }

    // base64 of the X.509 SubjectPublicKeyInfo of the meter key
    public required string PublicKey { get; init; }

    public required DateTimeOffset EnrolledAt { get; init; }
}