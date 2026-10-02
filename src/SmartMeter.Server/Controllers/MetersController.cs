using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;

namespace SmartMeter.Api.Controllers;

// everything a caller can read about one meter, without needing a live connection
// this is the half of the api that polls: grafana, a status page, a script
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

    // GET /api/v1/meters  ->  every meter the server has a reading from
    [HttpGet]
    public async Task<IReadOnlyList<string>> List(CancellationToken cancellationToken)
    {
        return await _store.GetMeterIdsAsync(cancellationToken);
    }

    // GET /api/v1/meters/meter-01/readings  ->  that meter's full history
    [HttpGet("{meterId}/readings")]
    public async Task<IReadOnlyList<Reading>> Readings(
        string meterId,
        CancellationToken cancellationToken)
    {
        return await _store.GetReadingsAsync(meterId, cancellationToken);
    }

    // GET /api/v1/meters/meter-01/bill  ->  what it owes right now
    [HttpGet("{meterId}/bill")]
    public async Task<Bill> Bill(string meterId, CancellationToken cancellationToken)
    {
        return await _billing.CalculateAsync(meterId, cancellationToken);
    }
}
