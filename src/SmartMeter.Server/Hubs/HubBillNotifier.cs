using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Hubs;

// sends bill updates to connected meter groups outside a hub method
// see https://learn.microsoft.com/aspnet/core/signalr/hubcontext
public sealed class HubBillNotifier : IBillNotifier
{
    private readonly IHubContext<MeterHub> _hub;

    public HubBillNotifier(IHubContext<MeterHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyMeterAsync(
        string meterId,
        Bill bill,
        CancellationToken cancellationToken = default)
    {
        await _hub.Clients.Group(meterId).SendAsync("BillUpdated", bill, cancellationToken);
    }
}
