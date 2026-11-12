
using Microsoft.AspNetCore.SignalR.Client;
using SmartMeter.Model;


public sealed class MeterServerClient : IAsyncDisposable // Disposable so we can close SignalR connection when done with it
{
    private readonly HubConnection _connection;
    private readonly TaskCompletionSource _readingsLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public MeterServerClient(
        Uri serverUri,
        string meterId,
        string accessToken)
    {
        Uri hubUri = new(serverUri, $"/api/v1/meters/{meterId}");

        _connection = new HubConnectionBuilder().WithUrl(
            hubUri,
            options =>
            {
                options.AccessTokenProvider =
                    () => Task.FromResult<string?>(accessToken);
            }).WithAutomaticReconnect().Build();// Tries to reconnect if communication issues 
        _connection.On<Bill>("BillUpdated", bill =>
        {
            Console.WriteLine($"Bill: {bill.Units} kWh at {bill.UnitPrice}/kWh = {bill.Total}");
        });
        _connection.On<GridAlert>("GridAlert", alert =>
        {
            Console.WriteLine($"Grid Alert: {alert.Message}");
        });
        _connection.On<string>("ReadingRejected", reason =>
        {
            Console.WriteLine($"Reading rejected: {reason}");
        });
        _connection.On<List<Reading>>("ReadingsLoaded", readings =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("====================================");
            Console.WriteLine("PREVIOUS READINGS");
            Console.WriteLine("====================================");

            if (readings.Count == 0)
            {
                Console.WriteLine("No previous readings found");
            }
            else
            {
                foreach (var reading in readings)
                {
                    Console.WriteLine($"[{reading.Timestamp:dd/MM/yyyy HH:mm:ss}] {reading.KilowattHours:F3} kWh");
                }
            }

            Console.WriteLine("====================================");
            Console.WriteLine();
            Console.ResetColor();
            _readingsLoaded.TrySetResult();
        });

    }

    public async Task ConnectAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _connection.StartAsync(cancellationToken);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Connected to server");
        Console.ResetColor();

        await _readingsLoaded.Task.WaitAsync(cancellationToken);
    }

    public async Task SendReadingAsync(Reading reading)
    {
        await _connection.InvokeAsync(
            "SubmitReading", reading);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
