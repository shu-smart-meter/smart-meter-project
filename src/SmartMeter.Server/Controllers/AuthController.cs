using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;
using SmartMeter.Server.Responses;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IAuthenticationExchange _exchange;

    public AuthController(IAuthenticationExchange exchange)
    {
        _exchange = exchange;
    }

    [HttpPost("token")]
    public async Task<ObjectResult> RequestToken(
        [FromBody] MeterTokenRequest request,
        CancellationToken cancellationToken)
    {
        TokenOutcome outcome = await _exchange.RequestTokenAsync(request, cancellationToken);

        if (outcome.Error is not null)
        {
            return new ObjectResult(ApiResponse.Failure(outcome.Error.Status, outcome.Error.Reason))
            {
                StatusCode = outcome.Error.Status,
            };
        }

        return new ObjectResult(outcome.Token ?? throw new IOException("token missing"));
    }
}
