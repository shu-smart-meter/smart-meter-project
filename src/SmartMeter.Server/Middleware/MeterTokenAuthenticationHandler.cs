using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

using SmartMeter.Model;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Middleware;

// validates the Authorization: Bearer token against the token service
// and issues a principal whose name is the meter id, with a role claim
public sealed class MeterTokenAuthenticationHandler : AuthenticationHandler<MeterTokenOptions>
{
    private readonly ITokenService _tokens;

    public MeterTokenAuthenticationHandler(
        IOptionsMonitor<MeterTokenOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokens)
        : base(options, logger, encoder)
    {
        _tokens = tokens;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? header = Request.Headers.Authorization;

        if (header is null)
        {
            return AuthenticateResult.NoResult();
        }

        string? token = BearerToken(header);

        if (token is null)
        {
            return AuthenticateResult.Fail("token not accepted");
        }

        if (_tokens.IsAdminToken(token))
        {
            // the admin token does not expire; leave the ticket without an expiry
            return AuthenticateResult.Success(TicketFor("admin", "admin", null));
        }

        TokenClaims? claims = await _tokens.ValidateAsync(token);

        if (claims is null)
        {
            return AuthenticateResult.Fail("token not accepted");
        }

        return AuthenticateResult.Success(TicketFor(claims.MeterId, "meter", claims.ExpiresAt));
    }

    // the value of the Authorization header, without the Bearer prefix
    private static string? BearerToken(string header)
    {
        const string scheme = "Bearer ";

        if (!header.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string token = header.Substring(scheme.Length).Trim();

        if (token.Length == 0)
        {
            return null;
        }

        return token;
    }

    // one identity per ticket; the expiry feeds CloseOnAuthenticationExpiration
    private static AuthenticationTicket TicketFor(string name, string role, DateTimeOffset? expiresAt)
    {
        // the meter id lives in the name claim; the identity itself carries
        // the authentication scheme that produced it
        Claim nameClaim = new Claim(ClaimsIdentity.DefaultNameClaimType, name);
        Claim roleClaim = new Claim(ClaimsIdentity.DefaultRoleClaimType, role);
        ClaimsIdentity identity = new ClaimsIdentity(
            new Claim[] { nameClaim, roleClaim },
            MeterTokenOptions.DefaultScheme,
            ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        AuthenticationProperties properties = new AuthenticationProperties();

        if (expiresAt is not null)
        {
            properties.ExpiresUtc = expiresAt;
        }

        return new AuthenticationTicket(principal, properties, MeterTokenOptions.DefaultScheme);
    }
}