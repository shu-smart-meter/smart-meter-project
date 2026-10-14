using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public sealed class AuthenticationExchange : IAuthenticationExchange
{
    private static readonly int MaximumMeterIdLength = 128;
    private static readonly int MaximumClientSecretLength = 512;

    private readonly AuthConfig _config;
    private readonly ITokenService _tokens;

    public AuthenticationExchange(AuthConfig config, ITokenService tokens)
    {
        _config = config;
        _tokens = tokens;
    }

    public async Task<TokenOutcome> RequestTokenAsync(
        MeterTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MeterId) ||
            request.MeterId.Length > MaximumMeterIdLength ||
            string.IsNullOrEmpty(request.ClientSecret) ||
            request.ClientSecret.Length > MaximumClientSecretLength ||
            !_config.AcceptsClientSecret(request.ClientSecret))
        {
            return Refuse(401, "credentials not accepted");
        }

        TokenResponse? token = await _tokens.IssueAsync(request.MeterId, cancellationToken);
        if (token is null)
        {
            return Refuse(503, "try again later");
        }

        return new TokenOutcome { Token = token };
    }

    private static TokenOutcome Refuse(int status, string reason)
    {
        return new TokenOutcome { Error = new AuthError { Status = status, Reason = reason } };
    }
}
