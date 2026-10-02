using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// checks each reading before the server stores it
public sealed class VerificationService : IVerificationService
{
    // maximum reading allowed in one 15- to 60-second interval
    private const decimal MaximumReadingKwh = 100m;

    // allow for small differences between the meter and server clocks
    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    public VerificationResult Verify(Reading reading)
    {
        if (string.IsNullOrWhiteSpace(reading.MeterId))
        {
            return VerificationResult.Reject("meter id is missing");
        }

        if (reading.KilowattHours < 0m)
        {
            return VerificationResult.Reject("reading cannot be negative");
        }

        if (reading.KilowattHours > MaximumReadingKwh)
        {
            return VerificationResult.Reject("reading is larger than a meter could send in one interval");
        }

        if (reading.Timestamp > DateTime.UtcNow.Add(ClockTolerance))
        {
            return VerificationResult.Reject("reading is dated in the future");
        }

        return VerificationResult.Accept();
    }
}
