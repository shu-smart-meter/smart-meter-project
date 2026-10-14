using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public interface IAuthenticationExchange
{
    Task<TokenOutcome> RequestTokenAsync(
        MeterTokenRequest request,
        CancellationToken cancellationToken = default);
}
