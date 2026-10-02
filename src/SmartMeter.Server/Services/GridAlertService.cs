using SmartMeter.Model;

namespace SmartMeter.Implementations;

// raises grid alerts, keeps them, and passes each one on to the meters
public sealed class GridAlertService : IGridAlertService
{
    private readonly IAlertNotifier _notifier;
    private readonly List<GridAlert> _alerts = new();

    // several alerts can be raised at once and they all land on the same list
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
            // hand back a copy so a caller cannot reach into the live list
            IReadOnlyList<GridAlert> copy = _alerts.ToList();
            return Task.FromResult(copy);
        }
    }
}
