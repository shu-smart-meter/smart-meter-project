namespace SmartMeter.Api.Models;

// what a caller sends to say the grid has a problem
public sealed record RaiseAlertRequest
{
    public required string Message { get; init; }
}
