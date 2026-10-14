namespace SmartMeter.Model;

// message about a grid problem
public sealed record GridAlert
{
    public required string Message { get; init; }

    public required DateTimeOffset RaisedAt { get; init; }
}
