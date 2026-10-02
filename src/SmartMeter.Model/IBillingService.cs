namespace SmartMeter.Model;

// works out what a meter owes from the readings we have stored
public interface IBillingService
{
    Task<Bill> CalculateAsync(string meterId, CancellationToken cancellationToken = default);
}
