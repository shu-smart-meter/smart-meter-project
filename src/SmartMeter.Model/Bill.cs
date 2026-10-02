namespace SmartMeter.Model;

// what the server sends back after it has accepted a reading
public sealed record Bill
{
    public required string MeterId { get; init; }

    // total kWh across every reading stored for this meter
    public required decimal Units { get; init; }

    // price per kWh at the moment the bill was worked out
    public required decimal UnitPrice { get; init; }

    public required decimal Total { get; init; }

    public required DateTimeOffset CalculatedAt { get; init; }
}
