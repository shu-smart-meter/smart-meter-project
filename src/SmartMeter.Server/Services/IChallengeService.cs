using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// issues and consumes the one-use signing challenges
public interface IChallengeService
{
    // issue a one-use challenge bound to a meter and its public key;
    // null when the server refuses to issue one, for example at capacity
    Task<ChallengeResponse?> CreateAsync(
        string meterId,
        byte[] publicKey,
        bool pendingEnrolment,
        CancellationToken cancellationToken = default);

    // consume a challenge; null when unknown, expired or already used
    Task<StoredChallenge?> ConsumeAsync(string challengeId, CancellationToken cancellationToken = default);
}