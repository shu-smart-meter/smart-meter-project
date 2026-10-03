using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;
using SmartMeter.Server.Responses;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Controllers;

// the public key authentication exchange
[Route("api/v1/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IAuthenticationExchange _exchange;

    public AuthController(IAuthenticationExchange exchange)
    {
        _exchange = exchange;
    }

    // POST /api/v1/auth/request asks for a challenge;
    // the failure branch is already an envelope, so the filter leaves it alone
    [HttpPost("request")]
    public async Task<ObjectResult> RequestChallenge(
        [FromBody] ChallengeRequest request,
        CancellationToken cancellationToken)
    {
        ChallengeOutcome outcome = await _exchange.RequestChallengeAsync(request, cancellationToken);

        if (outcome.Error is not null)
        {
            return new ObjectResult(ApiResponse.Failure(outcome.Error.Status, outcome.Error.Reason))
            {
                StatusCode = outcome.Error.Status,
            };
        }

        // the exchange guarantees a challenge when there is no error
        return new ObjectResult(outcome.Challenge ?? throw new IOException("challenge missing"));
    }

    // POST /api/v1/auth/prove signs the challenge and receives an access token
    [HttpPost("prove")]
    public async Task<ObjectResult> Prove(
        [FromBody] ProofRequest request,
        CancellationToken cancellationToken)
    {
        ProofOutcome outcome = await _exchange.ProveAsync(request, cancellationToken);

        if (outcome.Error is not null)
        {
            return new ObjectResult(ApiResponse.Failure(outcome.Error.Status, outcome.Error.Reason))
            {
                StatusCode = outcome.Error.Status,
            };
        }

        // the exchange guarantees a token when there is no error
        return new ObjectResult(outcome.Token ?? throw new IOException("token missing"));
    }
}