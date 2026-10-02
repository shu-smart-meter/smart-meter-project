using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// stores grid alerts and sends them to connected meters
public sealed class GridAlertService : IGridAlertService
{
    private readonly IAlertNotifier _notifier;
    private readonly List<GridAlert> _alerts = new();

    // protect the alert list when requests run at the same time
    private readonly object _gate = new();

    public GridAlertService(IAlertNotifier notifier)
    {
        _notifier = notifier;
    }

    public async Task<GridAlert> RaiseAsync(string message, CancellationToken cancellationToken = default)
    {
        GridAlert alert = new GridAlert { Message = message, RaisedAt = DateTimeOffset.UtcNow };

        lock (_gate)
        {
            _alerts.Add(alert);
        }

        await _notifier.NotifyAllAsync(alert, cancellationToken);

        return alert;
    }

    public Task<IReadOnlyList<GridAlert>> GetAlertsAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            // return a copy so callers cannot change the stored list
            IReadOnlyList<GridAlert> copy = _alerts.ToList();
            return Task.FromResult(copy);
        }
    }
}
