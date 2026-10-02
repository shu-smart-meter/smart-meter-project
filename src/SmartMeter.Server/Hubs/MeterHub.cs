using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;

namespace SmartMeter.Api.Hubs;

// a meter dials its own address on the server, eg /api/v1/meters/meter-01
// that address is the meter's identity, so the payload does not have to repeat it
// and a meter cannot write under another meter's name
//
// the channel is two way: the meter pushes readings up it, and the server pushes
// bills and grid alerts back down it. signalr is the plumbing that makes one tcp
// connection do both: https://learn.microsoft.com/aspnet/core/signalr/hubs
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

    // runs when the meter first connects. it gets back whatever readings the server
    // already holds, so a meter that restarts picks up where it left off
    public override async Task OnConnectedAsync()
    {
        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(MeterId);

        await Clients.Caller.SendAsync("ReadingsLoaded", readings);

        await base.OnConnectedAsync();
    }

    // a meter pushes one reading here every 15 to 60 seconds
    public async Task SubmitReading(Reading reading)
    {
        // the meter name comes from the address it dialled, never from the payload
        Reading stamped = reading with { MeterId = MeterId };

        VerificationResult result = _verification.Verify(stamped);

        if (!result.IsAccepted)
        {
            // this meter only. broadcasting would tell every meter on the system
            await Clients.Caller.SendAsync("ReadingRejected", result.Reason);
            return;
        }

        await _store.SaveAsync(stamped);

        Bill bill = await _billing.CalculateAsync(MeterId);

        await Clients.Caller.SendAsync("BillUpdated", bill);
    }

    // /api/v1/meters/meter-01  ->  "meter-01"
    private string MeterId
    {
        get
        {
            string? id = Context.GetHttpContext()?.Request.RouteValues["meterId"]?.ToString();
            return id ?? string.Empty;
        }
    }
}
