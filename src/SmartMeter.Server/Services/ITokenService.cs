using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public interface ITokenService
{
    Task<TokenResponse?> IssueAsync(string meterId, CancellationToken cancellationToken = default);

    Task<TokenClaims?> ValidateAsync(string accessToken, CancellationToken cancellationToken = default);

    bool IsAdminToken(string token);
}
