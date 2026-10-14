using SmartMeter.Model;

namespace SmartMeter.Server.Storage;

// storage interface for meter readings
public interface IReadingStore
{
    // save a reading
    Task SaveAsync(Reading reading, CancellationToken cancellationToken = default);

    // return a meter's readings in timestamp order
    Task<IReadOnlyList<Reading>> GetReadingsAsync(string meterId, CancellationToken cancellationToken = default);

    // return IDs of meters with stored readings
    Task<IReadOnlyList<string>> GetMeterIdsAsync(CancellationToken cancellationToken = default);
}
