using System.Globalization;

using Microsoft.Extensions.Configuration;

using SmartMeter.Model;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Services;

// calculates a bill from a meter's stored readings
public sealed class BillingService : IBillingService
{
    private readonly IReadingStore _store;
    private readonly IConfiguration _configuration;

    public BillingService(IReadingStore store, IConfiguration configuration)
    {
        _store = store;
        _configuration = configuration;
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

    private decimal GetCurrentUnitPrice()
    {
        string? configuredUnitPrice = _configuration["Billing:UnitPricePerKwh"];

        if (!decimal.TryParse(
                configuredUnitPrice,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal unitPrice))
        {
            throw new InvalidOperationException(
                "Billing:UnitPricePerKwh must be a valid non-negative decimal value.");
        }

        if (unitPrice < 0)
        {
            throw new InvalidOperationException(
                "Billing:UnitPricePerKwh must be a valid non-negative decimal value.");
        }

        return unitPrice;
    }
}
