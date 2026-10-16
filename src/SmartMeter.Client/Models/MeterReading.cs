namespace SmartMeter.Client.Services;

public class MeterReading
{
    public string MeterId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public decimal KilowattHours { get; set; }
}
