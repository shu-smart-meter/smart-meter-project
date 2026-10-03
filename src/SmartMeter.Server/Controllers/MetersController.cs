using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using SmartMeter.Model;
using SmartMeter.Server.Responses;
using SmartMeter.Server.Services;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Controllers;

// endpoints for stored meter readings and bills; each meter sees only its own
[Authorize]
[Route("api/v1/meters")]
public sealed class MetersController : BaseApiController
{
    private readonly IReadingStore _store;
    private readonly IBillingService _billing;

    public MetersController(IReadingStore store, IBillingService billing)
    {
        _store = store;
        _billing = billing;
    }

    // GET /api/v1/meters is the global inventory; admins only
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ObjectResult> List(CancellationToken cancellationToken)
    {
        IReadOnlyList<string> meterIds = await _store.GetMeterIdsAsync(cancellationToken);

        return new ObjectResult(meterIds);
    }

    // GET /api/v1/meters/meter-01/readings lists that meter's readings;
    // the route and the token claim must name the same meter
    [HttpGet("{meterId}/readings")]
    public async Task<ObjectResult> Readings(string meterId, CancellationToken cancellationToken)
    {
        if (!CanReadMeter(meterId))
        {
            return Forbidden();
        }

        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(meterId, cancellationToken);

        return new ObjectResult(readings);
    }

    // GET /api/v1/meters/meter-01/bill returns that meter's bill
    [HttpGet("{meterId}/bill")]
    public async Task<ObjectResult> Bill(string meterId, CancellationToken cancellationToken)
    {
        if (!CanReadMeter(meterId))
        {
            return Forbidden();
        }

        Bill bill = await _billing.CalculateAsync(meterId, cancellationToken);

        return new ObjectResult(bill);
    }

    // admins read anything; a meter token reads the meter it names exactly
    private bool CanReadMeter(string meterId)
    {
        ClaimsPrincipal? user = User;

        if (user is null)
        {
            return false;
        }

        string? name = user.Identity?.Name;

        if (name is null)
        {
            return false;
        }

        if (HasRole(user, "admin"))
        {
            return true;
        }

        return HasRole(user, "meter") && name.Equals(meterId);
    }

    private static bool HasRole(ClaimsPrincipal user, string role)
    {
        Claim? claim = user.FindFirst(ClaimTypes.Role);

        return claim is not null && claim.Value.Equals(role);
    }

    private static ObjectResult Forbidden()
    {
        return new ObjectResult(ApiResponse.Failure(403, "not allowed"))
        {
            StatusCode = 403,
        };
    }
}