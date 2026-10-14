using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public interface IBillCalculationStrategy
{
    Bill Calculate(string meterId, IReadOnlyList<Reading> readings, decimal unitPrice);
}
