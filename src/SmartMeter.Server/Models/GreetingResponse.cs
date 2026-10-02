namespace SmartMeter.Api.Models;

public sealed record GreetingResponse
{
    public required string Message { get; init; }
}
