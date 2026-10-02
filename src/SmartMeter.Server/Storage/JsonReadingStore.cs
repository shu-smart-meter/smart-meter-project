using System.Text.Json;

using SmartMeter.Model;

namespace SmartMeter.Server.Storage;

// stores readings in a JSON file so they remain available after a restart
public sealed class JsonReadingStore : IReadingStore
{
    // one JSON object per meter, for example { "meter-01": [ ...readings ] }
    private readonly Dictionary<string, List<Reading>> _readings = new();

    // path of the file used to load and save readings
    private readonly string _path;

    // prevent simultaneous writes to the file
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonReadingStore(string path)
    {
        _path = path;
        Load();
    }

    public async Task SaveAsync(Reading reading, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (!_readings.TryGetValue(reading.MeterId, out List<Reading>? existing))
            {
                existing = new List<Reading>();
                _readings[reading.MeterId] = existing;
            }

            existing.Add(reading);
            await WriteToFileAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<Reading>> GetReadingsAsync(
        string meterId,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (!_readings.TryGetValue(meterId, out List<Reading>? existing))
            {
                return Array.Empty<Reading>();
            }

            // sort readings so billing does not depend on arrival order
            return existing.OrderBy(reading => reading.Timestamp).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<string>> GetMeterIdsAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return _readings.Keys.OrderBy(meterId => meterId).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    // load saved readings when the store starts
    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        string json = File.ReadAllText(_path);

        Dictionary<string, List<Reading>>? stored =
            JsonSerializer.Deserialize<Dictionary<string, List<Reading>>>(json);

        if (stored is null)
        {
            return;
        }

        foreach (KeyValuePair<string, List<Reading>> pair in stored)
        {
            _readings[pair.Key] = pair.Value;
        }
    }

    private async Task WriteToFileAsync(CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(
            _readings,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_path, json, cancellationToken);
    }
}
