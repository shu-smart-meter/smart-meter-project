using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;
using SmartMeter.Server.Services;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Hubs;

// meters connect to /api/v1/meters/{meterId}
// readings go to the server through this hub; bills and alerts return through it
public sealed class MeterHub : Hub
{
    private readonly IReadingStore _store;
    private readonly IVerificationService _verification;
    private readonly IBillingService _billing;

    public MeterHub(
        IReadingStore store,
        IVerificationService verification,
        IBillingService billing)
    {
        _store = store;
        _verification = verification;
        _billing = billing;
    }

    // send stored readings when a meter connects
    public override async Task OnConnectedAsync()
    {
        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(MeterId);

        await Clients.Caller.SendAsync("ReadingsLoaded", readings);

        await base.OnConnectedAsync();
    }

    // receive a reading from the connected meter
    public async Task SubmitReading(Reading reading)
    {
        // use the meter ID from the route, not from the submitted reading
        Reading stamped = reading with { MeterId = MeterId };

        VerificationResult result = _verification.Verify(stamped);

        if (!result.IsAccepted)
        {
            // send the rejection only to the meter that submitted the reading
            await Clients.Caller.SendAsync("ReadingRejected", result.Reason);
            return;
        }

        await _store.SaveAsync(stamped);

        Bill bill = await _billing.CalculateAsync(MeterId);

        await Clients.Caller.SendAsync("BillUpdated", bill);
    }

    // read the meter ID from the hub route
    private string MeterId
    {
        get
        {
            string? id = Context.GetHttpContext()?.Request.RouteValues["meterId"]?.ToString();
            return id ?? string.Empty;
        }
    }
}
