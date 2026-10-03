using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// the public key authentication exchange; controllers see only this interface
public interface IAuthenticationExchange
{
    // step one: ask for a challenge, optionally with the enrolment secret
    Task<ChallengeOutcome> RequestChallengeAsync(
        ChallengeRequest request,
        CancellationToken cancellationToken = default);

    // step two: prove the challenge was signed by the registered key
    Task<ProofOutcome> ProveAsync(ProofRequest request, CancellationToken cancellationToken = default);
}