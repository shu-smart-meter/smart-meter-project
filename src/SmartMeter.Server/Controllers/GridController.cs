using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;
using SmartMeter.Server.Models;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Controllers;

// grid alert endpoints
[Authorize]
[Route("api/v1/grid")]
public sealed class GridController : BaseApiController
{
    private readonly IGridAlertService _alerts;

    public GridController(IGridAlertService alerts)
    {
        _alerts = alerts;
    }

    // GET /api/v1/grid/alerts lists alerts; any authenticated principal
    [HttpGet("alerts")]
    public async Task<ObjectResult> List(CancellationToken cancellationToken)
    {
        IReadOnlyList<GridAlert> alerts = await _alerts.GetAlertsAsync(cancellationToken);

        return new ObjectResult(alerts);
    }

    // POST /api/v1/grid/alerts raises an alert and pushes it to meters; admins only
    [Authorize(Roles = "admin")]
    [HttpPost("alerts")]
    public async Task<ObjectResult> Raise(
        [FromBody] RaiseAlertRequest request,
        CancellationToken cancellationToken)
    {
        GridAlert alert = await _alerts.RaiseAsync(request.Message, cancellationToken);

        return new ObjectResult(alert);
    }
}