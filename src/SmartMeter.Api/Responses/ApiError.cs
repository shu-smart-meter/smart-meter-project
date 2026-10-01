namespace SmartMeter.Api.Responses;

public sealed record ApiError
{
    public required ApiErrorCode Error { get; init; }

    public required string Message { get; init; }
}
