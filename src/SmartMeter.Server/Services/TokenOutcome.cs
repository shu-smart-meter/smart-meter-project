using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public sealed record TokenOutcome
{
    public TokenResponse? Token { get; init; }

    public AuthError? Error { get; init; }
}

public sealed record AuthError
{
    public required int Status { get; init; }

    public required string Reason { get; init; }
}
