using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;
using SmartMeter.Server.Models;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Controllers;

// grid alert endpoints
[Route("api/v1/grid")]
public sealed class GridController : BaseApiController
{
    private readonly IGridAlertService _alerts;

    public GridController(IGridAlertService alerts)
    {
        _alerts = alerts;
    }

    // GET /api/v1/grid/alerts lists alerts
    [HttpGet("alerts")]
    public async Task<IReadOnlyList<GridAlert>> List(CancellationToken cancellationToken)
    {
        return await _alerts.GetAlertsAsync(cancellationToken);
    }

    // POST /api/v1/grid/alerts raises an alert and sends it to connected meters
    [HttpPost("alerts")]
    public async Task<GridAlert> Raise(
        [FromBody] RaiseAlertRequest request,
        CancellationToken cancellationToken)
    {
        return await _alerts.RaiseAsync(request.Message, cancellationToken);
    }
}
