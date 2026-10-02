using SmartMeter.Model;

namespace SmartMeter.Implementations;

// turns the readings we have stored into a bill
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

    // fixed for now so a bill is reproducible; a real service would take this from a price feed
    private static decimal GetCurrentUnitPrice()
    {
        return 0.24m;
    }
}
