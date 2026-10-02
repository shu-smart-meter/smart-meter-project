namespace SmartMeter.Server.Responses;

public sealed record ApiResponse<T>
{
    public required bool Ok { get; init; }

    public required int Status { get; init; }

    public required T Response { get; init; }
}
