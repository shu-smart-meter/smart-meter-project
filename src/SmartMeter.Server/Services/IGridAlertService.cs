using SmartMeter.Model;

namespace SmartMeter.Server.Services;

// knows when the grid has a problem and keeps a record of every alert it raised
public interface IGridAlertService
{
    // say the grid has a problem; every connected meter hears about it straight away
    Task<GridAlert> RaiseAsync(string message, CancellationToken cancellationToken = default);

    // every alert raised so far, oldest first
    Task<IReadOnlyList<GridAlert>> GetAlertsAsync(CancellationToken cancellationToken = default);
}
