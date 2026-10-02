namespace SmartMeter.Server.Services;

// result of checking whether a reading can be stored
// the reason tells the meter why the server rejected it
public sealed record VerificationResult
{
    public required bool IsAccepted { get; init; }

    // empty when the reading is accepted
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
