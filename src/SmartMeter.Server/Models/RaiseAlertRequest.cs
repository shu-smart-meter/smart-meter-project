namespace SmartMeter.Server.Models;

// what a caller sends to say the grid has a problem
public sealed record RaiseAlertRequest
{
    public required string Message { get; init; }
}
