using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// issues and consumes the one-use signing challenges
public interface IChallengeService
{
    // issue a one-use challenge bound to a meter
    Task<ChallengeResponse> CreateAsync(
        string meterId,
        bool pendingEnrolment,
        CancellationToken cancellationToken = default);

    // consume a challenge; null when unknown, expired or already used
    Task<StoredChallenge?> ConsumeAsync(string challengeId, CancellationToken cancellationToken = default);
}