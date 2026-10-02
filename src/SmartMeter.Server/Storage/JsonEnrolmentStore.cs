using System.Text.Json;

using SmartMeter.Model;

namespace SmartMeter.Server.Storage;

// stores enrolled client keys in a JSON file so reconnects survive a restart
public sealed class JsonEnrolmentStore : IEnrolmentStore
{
    // one record per meter, for example { "meter-01": { ... } }
    private readonly Dictionary<string, EnrolmentRecord> _clients = new();

    // path of the file used to load and save records
    private readonly string _path;

    // prevent simultaneous writes to the file
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonEnrolmentStore(string path)
    {
        _path = path;
        Load();
    }

    public async Task SaveAsync(EnrolmentRecord record, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            _clients[record.MeterId] = record;
            await WriteToFileAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<EnrolmentRecord?> FindAsync(
        string meterId,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (!_clients.TryGetValue(meterId, out EnrolmentRecord? record))
            {
                return null;
            }

            return record;
        }
        finally
        {
            _gate.Release();
        }
    }

    // load saved records when the store starts
    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        string json = File.ReadAllText(_path);

        Dictionary<string, EnrolmentRecord>? stored =
            JsonSerializer.Deserialize<Dictionary<string, EnrolmentRecord>>(json);

        if (stored is null)
        {
            return;
        }

        foreach (KeyValuePair<string, EnrolmentRecord> pair in stored)
        {
            _clients[pair.Key] = pair.Value;
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
            _clients,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_path, json, cancellationToken);
    }
}