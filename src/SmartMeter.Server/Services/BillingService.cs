using SmartMeter.Model;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Services;

// calculates a bill from a meter's stored readings
public sealed class BillingService : IBillingService
{
    private readonly IReadingStore _store;

    public BillingService(IReadingStore store)
    {
        _store = store;
    }

    public async Task<Bill> CalculateAsync(string meterId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(meterId, cancellationToken);

        decimal totalUnits = readings.Sum(reading => reading.KilowattHours);
        decimal unitPrice = GetCurrentUnitPrice();

        return new Bill
        {
            MeterId = meterId,
            Units = totalUnits,
            UnitPrice = unitPrice,
            Total = Math.Round(totalUnits * unitPrice, 2),
            CalculatedAt = DateTimeOffset.UtcNow,
        };
    }

    // use a fixed price until the server has a price source
    private static decimal GetCurrentUnitPrice()
    {
        return 0.24m;
    }
}
