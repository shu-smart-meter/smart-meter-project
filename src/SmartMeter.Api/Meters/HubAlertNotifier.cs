using Microsoft.AspNetCore.SignalR;

using SmartMeter.Api.Hubs;
using SmartMeter.Model;

namespace SmartMeter.Api.Meters;

// pushes a grid alert to every meter that is connected right now
//
// signalr will only talk to clients from inside a hub, or through this HubContext when
// the code lives somewhere else. that is the whole reason this class exists.
// why: https://learn.microsoft.com/aspnet/core/signalr/hubcontext
public sealed class HubAlertNotifier : IAlertNotifier
{
    private readonly IHubContext<MeterHub> _hub;

    public HubAlertNotifier(IHubContext<MeterHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyAllAsync(GridAlert alert, CancellationToken cancellationToken = default)
    {
        // Clients.All is every connected meter at once
        await _hub.Clients.All.SendAsync("GridAlert", alert, cancellationToken);
    }
}
