using Microsoft.AspNetCore.Mvc;

using SmartMeter.Model;
using SmartMeter.Server.Services;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Controllers;

// endpoints for stored meter readings and bills
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

    // GET /api/v1/meters returns meters with readings
    [HttpGet]
    public async Task<IReadOnlyList<string>> List(CancellationToken cancellationToken)
    {
        return await _store.GetMeterIdsAsync(cancellationToken);
    }

    // GET /api/v1/meters/meter-01/readings lists that meter's readings
    [HttpGet("{meterId}/readings")]
    public async Task<IReadOnlyList<Reading>> Readings(
        string meterId,
        CancellationToken cancellationToken)
    {
        return await _store.GetReadingsAsync(meterId, cancellationToken);
    }

    // GET /api/v1/meters/meter-01/bill returns that meter's bill
    [HttpGet("{meterId}/bill")]
    public async Task<Bill> Bill(string meterId, CancellationToken cancellationToken)
    {
        return await _billing.CalculateAsync(meterId, cancellationToken);
    }
}
