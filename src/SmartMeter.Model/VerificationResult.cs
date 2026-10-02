namespace SmartMeter.Model;

// the answer to "is this reading safe to store?"
// kept as its own type rather than a plain bool so the caller can tell the meter why it was turned down
public sealed record VerificationResult
{
    public required bool IsAccepted { get; init; }

    // empty when the reading was accepted
    public required string Reason { get; init; }

    public static VerificationResult Accept()
    {
        return new VerificationResult { IsAccepted = true, Reason = string.Empty };
    }

    public static VerificationResult Reject(string reason)
    {
        return new VerificationResult { IsAccepted = false, Reason = reason };
    }
}
