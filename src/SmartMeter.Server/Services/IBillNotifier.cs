using SmartMeter.Model;

namespace SmartMeter.Server.Services;

public interface IBillNotifier
{
    Task NotifyMeterAsync(string meterId, Bill bill, CancellationToken cancellationToken = default);
}
