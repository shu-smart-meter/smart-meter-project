namespace SmartMeter.Model;

// something went wrong on the grid; pushed to every connected meter
public sealed record GridAlert
{
    public required string Message { get; init; }

    public required DateTimeOffset RaisedAt { get; init; }
}
