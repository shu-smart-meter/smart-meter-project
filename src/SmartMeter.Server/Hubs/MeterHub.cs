using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;
using SmartMeter.Server.Services;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Hubs;

// meters connect to /api/v1/meters/{meterId} with their bearer token
// the route names the resource; the token claim must match it
// readings go to the server through this hub; bills and alerts return through it
public sealed class MeterHub : Hub
{
    private readonly IReadingStore _store;
    private readonly IVerificationService _verification;
    private readonly IBillingService _billing;
    private readonly ILogger<MeterHub> _logger;

    public MeterHub(
        IReadingStore store,
        IVerificationService verification,
        IBillingService billing,
        ILogger<MeterHub> logger)
    {
        _store = store;
        _verification = verification;
        _billing = billing;
        _logger = logger;
    }

    // send stored readings when a meter connects
    public override async Task OnConnectedAsync()
    {
        IReadOnlyList<Reading> readings = await _store.GetReadingsAsync(MeterId);

        await Clients.Caller.SendAsync("ReadingsLoaded", readings);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "Meter {MeterId} connection {ConnectionId} disconnected with an error",
                MeterId,
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "Meter {MeterId} connection {ConnectionId} disconnected",
                MeterId,
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // receive a reading from the connected meter
    public async Task SubmitReading(Reading reading)
    {
        // identity comes from the token claim, not from the submitted payload
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

    // the meter the token claim names; the connection can act as that meter only
    private string MeterId
    {
        get
        {
            return Context.User?.Identity?.Name ?? string.Empty;
        }
    }
}
