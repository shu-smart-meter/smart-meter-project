namespace SmartMeter.Model;

// one reading sent by one meter
public sealed record Reading
{
    // which meter sent it, eg "meter-01"
    public required string MeterId { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    // electricity used since the last reading, in kWh
    public required decimal KilowattHours { get; init; }
}
