namespace SmartMeter.Model;

// where readings go. sits behind an interface so the tests can swap in a fake
// and so the real storage can be swapped without touching the hub
public interface IReadingStore
{
    // save one reading for one meter
    Task SaveAsync(Reading reading, CancellationToken cancellationToken = default);

    // every reading we hold for a meter, oldest first; used to build the bill
    Task<IReadOnlyList<Reading>> GetReadingsAsync(string meterId, CancellationToken cancellationToken = default);

    // all meters we know about, so the server can reattach them at start-up
    Task<IReadOnlyList<string>> GetMeterIdsAsync(CancellationToken cancellationToken = default);
}
