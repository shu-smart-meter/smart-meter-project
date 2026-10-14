public class MeterReading
{
    public Guid ReadingId { get; set; }
    public string MeterId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double Reading { get; set; }
}