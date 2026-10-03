using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// outcome of a challenge request; exactly one of Challenge or Error is set
public sealed record ChallengeOutcome
{
    public ChallengeResponse? Challenge { get; init; }

    public AuthError? Error { get; init; }
}

// outcome of a proof; exactly one of Token or Error is set
public sealed record ProofOutcome
{
    public TokenResponse? Token { get; init; }

    public AuthError? Error { get; init; }
}

// a public refusal that maps to an http status and a short reason
public sealed record AuthError
{
    public required int Status { get; init; }

    public required string Reason { get; init; }
}