using System.Security.Cryptography;
using System.Text;

namespace SmartMeter.Server.Services;

// settings for the authentication exchange
// secrets come from environment variables, never from tracked config
public sealed class AuthConfig
{
    private readonly string? _enrolmentSecret;
    private readonly byte[]? _adminTokenHash;
    private readonly TimeSpan _tokenLifetime;
    private readonly TimeSpan _challengeLifetime;

    public AuthConfig(
        string? enrolmentSecret,
        byte[]? adminTokenHash,
        TimeSpan tokenLifetime,
        TimeSpan challengeLifetime)
    {
        _enrolmentSecret = enrolmentSecret;
        _adminTokenHash = adminTokenHash;
        _tokenLifetime = tokenLifetime;
        _challengeLifetime = challengeLifetime;
    }

    // a ttl of zero or a negative value would break token expiry
    private static readonly long MaximumLifetimeSeconds = 31L * 24 * 60 * 60;

    // secrets win from the environment; ttl falls back to appsettings, then defaults
    public static AuthConfig From(IConfiguration configuration)
    {
        string? enrolmentSecret = Environment.GetEnvironmentVariable("SMART_METER_ENROLMENT_SECRET");
        string? adminToken = Environment.GetEnvironmentVariable("SMART_METER_ADMIN_TOKEN");

        long tokenSeconds = ReadLong(
            Environment.GetEnvironmentVariable("SMART_METER_TOKEN_SECONDS"),
            ReadLong(configuration["Auth:TokenLifetimeSeconds"], 3600));
        long challengeSeconds = ReadLong(
            Environment.GetEnvironmentVariable("SMART_METER_CHALLENGE_SECONDS"),
            ReadLong(configuration["Auth:ChallengeLifetimeSeconds"], 120));

        return new AuthConfig(
            enrolmentSecret,
            HashToken(adminToken),
            TimeSpan.FromSeconds(tokenSeconds),
            TimeSpan.FromSeconds(challengeSeconds));
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

    // shared credential a new key must present once; null = enrolment closed
    public string? EnrolmentSecret
    {
        get
        {
            return _enrolmentSecret;
        }
    }

    // hash of the admin token, so the raw value is not kept around
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

    public TimeSpan ChallengeLifetime
    {
        get
        {
            return _challengeLifetime;
        }
    }

    public static byte[]? HashToken(string? token)
    {
        if (token is null || token.Length == 0)
        {
            return null;
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }
}