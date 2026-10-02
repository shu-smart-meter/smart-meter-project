using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;

namespace SmartMeter.Api.Hubs;

// the connection point for every meter
// a meter calls SendReading, and this pushes a bill back down the same connection
// why: https://learn.microsoft.com/aspnet/core/signalr/hubs
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

    // one meter sends one reading; this runs on the server each time it arrives
    // the meter id comes in the payload, so we never have to guess who sent it
    public async Task SendReading(Reading reading)
    {
        // check before storing, so a bad reading never gets near the data
        VerificationResult result = _verification.Verify(reading);

        if (!result.IsAccepted)
        {
            // send only to the meter that called us, not to every meter
            await Clients.Caller.SendAsync("ReadingRejected", result.Reason);
            return;
        }

        await _store.SaveAsync(reading);

        Bill bill = await _billing.CalculateAsync(reading.MeterId);

        // BillUpdated is the name the meter listens for
        await Clients.Caller.SendAsync("BillUpdated", bill);
    }
}
