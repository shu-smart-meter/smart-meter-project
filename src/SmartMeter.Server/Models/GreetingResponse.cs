namespace SmartMeter.Server.Models;

public sealed record GreetingResponse
{
    public required string Message { get; init; }
}
