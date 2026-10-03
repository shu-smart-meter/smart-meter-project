using System.Text;
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
            await WriteToFileAsync(record, cancellationToken);

            // memory changes only once the file is safely in place
            _clients[record.MeterId] = record;
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

    private async Task WriteToFileAsync(EnrolmentRecord record, CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // stage the new record beside the loaded ones, still in memory
        Dictionary<string, EnrolmentRecord> next = new(_clients);
        next[record.MeterId] = record;

        string json = JsonSerializer.Serialize(
            next,
            new JsonSerializerOptions { WriteIndented = true });

        // write beside the target and flush, then move, so a crash mid-write
        // never leaves a half written file where the store expects one
        string tempPath = _path + ".tmp";

        {
            // disposal before the move closes the handle on the temp file
            await using FileStream writer = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await writer.WriteAsync(Encoding.UTF8.GetBytes(json), cancellationToken);
            await writer.FlushAsync(cancellationToken);
            writer.Flush(true);
        }

        File.Move(tempPath, _path, true);
    }
}