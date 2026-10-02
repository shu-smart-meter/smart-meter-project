using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// checks a reading before it is stored
public interface IVerificationService
{
    VerificationResult Verify(Reading reading);
}
