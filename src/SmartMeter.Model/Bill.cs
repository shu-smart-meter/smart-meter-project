namespace SmartMeter.Model;

// bill details from a meter's stored readings
public sealed record Bill
{
    public required string MeterId { get; init; }

    // total kWh across all readings stored for this meter
    public required decimal Units { get; init; }

    // price per kWh used to calculate the total
    public required decimal UnitPrice { get; init; }

    public required decimal Total { get; init; }

    public required DateTimeOffset CalculatedAt { get; init; }
}
