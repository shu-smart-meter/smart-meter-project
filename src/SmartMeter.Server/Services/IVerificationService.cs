namespace SmartMeter.Model;

// checks a reading before it is stored
public interface IVerificationService
{
    VerificationResult Verify(Reading reading);
}
