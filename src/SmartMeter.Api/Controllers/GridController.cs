using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Models;
using SmartMeter.Model;

namespace SmartMeter.Api.Controllers;

// the grid side of the api: what has gone wrong, and a way to say something has
[Route("api/v1/grid")]
public sealed class GridController : BaseApiController
{
    private readonly IGridAlertService _alerts;

    public GridController(IGridAlertService alerts)
    {
        _alerts = alerts;
    }

    // GET /api/v1/grid/alerts  ->  every alert so far
    [HttpGet("alerts")]
    public async Task<IReadOnlyList<GridAlert>> List(CancellationToken cancellationToken)
    {
        return await _alerts.GetAlertsAsync(cancellationToken);
    }

    // POST /api/v1/grid/alerts  ->  say the grid has a problem. every meter that is
    // connected gets pushed the alert straight away, as well as it being stored here
    [HttpPost("alerts")]
    public async Task<GridAlert> Raise(
        [FromBody] RaiseAlertRequest request,
        CancellationToken cancellationToken)
    {
        return await _alerts.RaiseAsync(request.Message, cancellationToken);
    }
}
