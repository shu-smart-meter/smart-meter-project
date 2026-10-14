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
    private readonly IBillCalculationStrategy _calculationStrategy;

    public BillingService(
        IReadingStore store,
        IConfiguration configuration,
        IBillCalculationStrategy calculationStrategy)
    {
        _store = store;
        _configuration = configuration;
        _calculationStrategy = calculationStrategy;
    }

    public async Task<Bill> CalculateAsync(string meterId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(meterId, cancellationToken);

        decimal unitPrice = GetCurrentUnitPrice();

        return _calculationStrategy.Calculate(meterId, readings, unitPrice);
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
