using System.Security.Cryptography;

using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// one-use signing challenges: random nonce, server-side expiry, bound to a key
public sealed class ChallengeService : IChallengeService
{
    // hard bound on outstanding challenges; pruned first, then refused
    private static readonly int MaximumChallenges = 1000;

    // challenge id -> the challenge and when it stops being acceptable
    private readonly Dictionary<string, ChallengeEntry> _challenges = new();
    private readonly AuthConfig _config;

    // guard the challenge map
    private readonly object _gate = new();

    public ChallengeService(AuthConfig config)
    {
        _config = config;
    }

    public Task<ChallengeResponse?> CreateAsync(
        string meterId,
        byte[] publicKey,
        bool pendingEnrolment,
        CancellationToken cancellationToken = default)
    {
        string challengeId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        byte[] nonce = RandomNumberGenerator.GetBytes(32);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(_config.ChallengeLifetime);

        lock (_gate)
        {
            PruneExpired();

            if (_challenges.Count >= MaximumChallenges)
            {
                return Task.FromResult<ChallengeResponse?>(null);
            }

            _challenges[challengeId] = new ChallengeEntry
            {
                Challenge = new StoredChallenge
                {
                    MeterId = meterId,
                    PublicKey = publicKey,
                    Nonce = nonce,
                    PendingEnrolment = pendingEnrolment,
                },
                ExpiresAt = expiresAt,
            };
        }

        return Task.FromResult<ChallengeResponse?>(new ChallengeResponse
        {
            ChallengeId = challengeId,
            Nonce = AuthCodec.EncodeBase64(nonce),
            ExpiresAt = expiresAt,
        });
    }

    public Task<StoredChallenge?> ConsumeAsync(
        string challengeId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            PruneExpired();

            if (!_challenges.TryGetValue(challengeId, out ChallengeEntry? entry))
            {
                return Task.FromResult<StoredChallenge?>(null);
            }

            // one-use: remove before returning so a replay cannot race the check
            _challenges.Remove(challengeId);

            if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return Task.FromResult<StoredChallenge?>(null);
            }

            return Task.FromResult<StoredChallenge?>(entry.Challenge);
        }
    }

    // remove expired entries so the map cannot grow without bound
    private void PruneExpired()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (KeyValuePair<string, ChallengeEntry> pair in _challenges.ToList())
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _challenges.Remove(pair.Key);
            }
        }
    }
}

// a challenge and the instant it stops being acceptable
public sealed record ChallengeEntry
{
    public required StoredChallenge Challenge { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}