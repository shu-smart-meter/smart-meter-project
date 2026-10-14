using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public sealed class FlatRateBillCalculationStrategy : IBillCalculationStrategy
{
    public Bill Calculate(string meterId, IReadOnlyList<Reading> readings, decimal unitPrice)
    {
        decimal totalUnits = readings.Sum(reading => reading.KilowattHours);

        return new Bill
        {
            MeterId = meterId,
            Units = totalUnits,
            UnitPrice = unitPrice,
            Total = Math.Round(totalUnits * unitPrice, 2),
            CalculatedAt = DateTimeOffset.UtcNow,
        };
    }
}
