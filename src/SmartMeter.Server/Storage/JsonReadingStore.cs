using System.Text;
using System.Text.Json;

using SmartMeter.Model;

namespace SmartMeter.Server.Storage;

// stores readings in a JSON file so they remain available after a restart
public sealed class JsonReadingStore : IReadingStore
{
    // one JSON object per meter, for example { "meter-01": [ ...readings ] }
    private Dictionary<string, List<Reading>> _readings = new();

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
            Dictionary<string, List<Reading>> next = CreateNextReadings(reading);
            await WriteToFileAsync(next, cancellationToken);
            _readings = next;
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

    private Dictionary<string, List<Reading>> CreateNextReadings(Reading reading)
    {
        Dictionary<string, List<Reading>> next = new(_readings);

        if (!next.TryGetValue(reading.MeterId, out List<Reading>? existing))
        {
            existing = new List<Reading>();
        }

        List<Reading> meterReadings = new List<Reading>(existing);
        meterReadings.Add(reading);
        next[reading.MeterId] = meterReadings;
        return next;
    }

    private async Task WriteToFileAsync(
        Dictionary<string, List<Reading>> next,
        CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(
            next,
            new JsonSerializerOptions { WriteIndented = true });

        string tempPath = _path + ".tmp";

        try
        {
            await using (FileStream writer = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                byte[] contents = Encoding.UTF8.GetBytes(json);
                await writer.WriteAsync(contents, cancellationToken);
                await writer.FlushAsync(cancellationToken);
                writer.Flush(true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(tempPath, _path, true);
        }
        catch
        {
            // cleanup must not replace the write error
            try
            {
                File.Delete(tempPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            throw;
        }
    }
}
