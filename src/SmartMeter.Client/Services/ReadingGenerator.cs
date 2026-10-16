namespace SmartMeter.Client.Services;

using SmartMeter.Model;

public sealed class ReadingGenerator
{
    private readonly Random _random = new Random();

    public Reading GenerateReading(string meterId)
    {
        decimal consumption = (decimal)_random.NextDouble() * 0.5m;

        return new Reading
        {
            MeterId = meterId,
            Timestamp = DateTimeOffset.UtcNow,
            KilowattHours = decimal.Round(consumption, 3)
        };
    }

    public int GetNextDelaySeconds()
    {
        return _random.Next(1, 11);
    }
}
