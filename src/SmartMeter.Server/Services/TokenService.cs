using System.Security.Cryptography;
using System.Text;

using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// issues random expiring tokens and validates them by hash lookup
public sealed class TokenService : ITokenService
{
    // hard bound on live tokens; pruned first, then new issues are refused
    private static readonly int MaximumTokens = 10000;

    // hash of the token -> claims; the token value itself is never stored
    private readonly Dictionary<string, TokenClaims> _tokens = new();
    private readonly AuthConfig _config;

    // guard the token map
    private readonly object _gate = new();

    public TokenService(AuthConfig config)
    {
        _config = config;
    }

    public Task<TokenResponse?> IssueAsync(string meterId, CancellationToken cancellationToken = default)
    {
        string token = AuthCodec.EncodeBase64Url(RandomNumberGenerator.GetBytes(32));
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(_config.TokenLifetime);
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
        byte[]? expected = _config.AdminTokenHash;

        if (expected is null || token.Length == 0)
        {
            return false;
        }

        byte[] presented = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return CryptographicOperations.FixedTimeEquals(expected, presented);
    }

    // remove expired entries so the map cannot grow without bound
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