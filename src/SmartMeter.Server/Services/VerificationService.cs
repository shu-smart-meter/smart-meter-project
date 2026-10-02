using SmartMeter.Model;

namespace SmartMeter.Implementations;

// the rules a reading has to pass before we store it
public sealed class VerificationService : IVerificationService
{
    // a meter sends a reading every 15 to 60 seconds, so anything larger than this is a stuck or broken meter
    private const decimal MaximumReadingKwh = 100m;

    // allow for clocks on the meter being slightly out from the server's clock
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
