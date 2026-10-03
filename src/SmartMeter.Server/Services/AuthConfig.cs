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