using System.Security.Cryptography;
using System.Text;

namespace SmartMeter.Server.Services;

public sealed class AuthConfig
{
    private readonly byte[]? _clientSecretHash;
    private readonly byte[]? _adminTokenHash;
    private readonly TimeSpan _tokenLifetime;

    public AuthConfig(byte[]? clientSecretHash, byte[]? adminTokenHash, TimeSpan tokenLifetime)
    {
        _clientSecretHash = clientSecretHash;
        _adminTokenHash = adminTokenHash;
        _tokenLifetime = tokenLifetime;
    }

    private static readonly long MaximumLifetimeSeconds = 31L * 24 * 60 * 60;

    public static AuthConfig From(IConfiguration configuration)
    {
        string? clientSecret = Environment.GetEnvironmentVariable("SMART_METER_CLIENT_SECRET");
        string? adminToken = Environment.GetEnvironmentVariable("SMART_METER_ADMIN_TOKEN");
        long tokenSeconds = ReadLong(
            Environment.GetEnvironmentVariable("SMART_METER_TOKEN_SECONDS"),
            ReadLong(configuration["Auth:TokenLifetimeSeconds"], 3600));

        return new AuthConfig(
            HashSecret(clientSecret),
            HashSecret(adminToken),
            TimeSpan.FromSeconds(tokenSeconds));
    }

    public bool AcceptsClientSecret(string? presented)
    {
        return MatchesSecret(_clientSecretHash, presented);
    }

    public byte[]? AdminTokenHash
    {
        get
        {
            return _adminTokenHash;
        }
    }

    public TimeSpan TokenLifetime
    {
        get
        {
            return _tokenLifetime;
        }
    }

    public static bool MatchesSecret(byte[]? expected, string? presented)
    {
        if (expected is null || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        byte[] actual = HashSecret(presented)!;
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static byte[]? HashSecret(string? secret)
    {
        if (secret is null || secret.Length == 0)
        {
            return null;
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    private static long ReadLong(string? value, long fallback)
    {
        if (value is null || !Int64.TryParse(value, out long parsed) ||
            parsed <= 0 || parsed > MaximumLifetimeSeconds)
        {
            return fallback;
        }

        return parsed;
    }
}
