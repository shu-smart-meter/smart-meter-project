namespace SmartMeter.Client.Services;

public class ReadingGenerator
{
    private readonly Random _random = new Random();
    private double _currentReading;

    public ReadingGenerator(double startingReading)
    {
        _currentReading = startingReading;
    }

    public MeterReading GenerateReading(string meterId)
    {
        double consumption = _random.NextDouble() * 0.5;

        _currentReading += consumption;

        return new MeterReading
        {
            ReadingId = Guid.NewGuid(),
            MeterId = meterId,
            Timestamp = DateTime.UtcNow,
            Reading = _currentReading
        };
    }

    public int GetNextDelaySeconds()
    {
        return _random.Next(1, 3);
    }
}