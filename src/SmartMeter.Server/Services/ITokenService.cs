using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// issues and validates the opaque bearer tokens the api and hub use
public interface ITokenService
{
    // issue a fresh token for a meter; null when the server refuses, for example at capacity
    Task<TokenResponse?> IssueAsync(string meterId, CancellationToken cancellationToken = default);

    // validate a presented token; null when unknown or expired
    Task<TokenClaims?> ValidateAsync(string accessToken, CancellationToken cancellationToken = default);

    // true when the presented token is the configured admin token
    bool IsAdminToken(string token);
}