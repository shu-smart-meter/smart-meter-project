using System.Security.Cryptography;
using System.Text;

using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public sealed class TokenService : ITokenService
{
    private static readonly int MaximumTokens = 10000;

    private readonly Dictionary<string, TokenClaims> _tokens = new();
    private readonly AuthConfig _config;

    private readonly object _gate = new();

    public TokenService(AuthConfig config)
    {
        _config = config;
    }

    public Task<TokenResponse?> IssueAsync(string meterId, CancellationToken cancellationToken = default)
    {
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(
            DateTimeOffset.UtcNow.Add(_config.TokenLifetime).ToUnixTimeSeconds());
        TokenClaims claims = new TokenClaims { MeterId = meterId, ExpiresAt = expiresAt };

        lock (_gate)
        {
            PruneExpired();

            if (_tokens.Count >= MaximumTokens)
            {
                return Task.FromResult<TokenResponse?>(null);
            }

            _tokens[HashToken(token)] = claims;
        }

        return Task.FromResult<TokenResponse?>(new TokenResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            MeterId = meterId,
        });
    }

    public Task<TokenClaims?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            PruneExpired();

            if (!_tokens.TryGetValue(HashToken(accessToken), out TokenClaims? claims))
            {
                return Task.FromResult<TokenClaims?>(null);
            }

            return Task.FromResult<TokenClaims?>(claims);
        }
    }

    public bool IsAdminToken(string token)
    {
        return AuthConfig.MatchesSecret(_config.AdminTokenHash, token);
    }

    private void PruneExpired()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (KeyValuePair<string, TokenClaims> pair in _tokens.ToList())
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _tokens.Remove(pair.Key);
            }
        }
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    }
}
