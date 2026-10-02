namespace SmartMeter.Model;

// a reading sent by a meter
public sealed record Reading
{
    // meter that sent the reading
    public required string MeterId { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    // electricity used since the previous reading, in kWh
    public required decimal KilowattHours { get; init; }
}
