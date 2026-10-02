namespace SmartMeter.Model;

// tells connected meters about something, without knowing how they are connected
public interface IAlertNotifier
{
    Task NotifyAllAsync(GridAlert alert, CancellationToken cancellationToken = default);
}
