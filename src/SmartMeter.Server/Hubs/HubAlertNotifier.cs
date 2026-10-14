using Microsoft.AspNetCore.SignalR;

using SmartMeter.Model;
using SmartMeter.Server.Services;

namespace SmartMeter.Server.Hubs;

// sends grid alerts to connected meters outside a hub method
// see https://learn.microsoft.com/aspnet/core/signalr/hubcontext
public sealed class HubAlertNotifier : IAlertNotifier
{
    private readonly IHubContext<MeterHub> _hub;

    public HubAlertNotifier(IHubContext<MeterHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyAllAsync(GridAlert alert, CancellationToken cancellationToken = default)
    {
        // clients.all targets every connected meter
        await _hub.Clients.All.SendAsync("GridAlert", alert, cancellationToken);
    }
}
