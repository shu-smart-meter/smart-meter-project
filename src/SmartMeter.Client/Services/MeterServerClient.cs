
using Microsoft.AspNetCore.SignalR.Client;
using SmartMeter.Model;


// Manages the SignalR connection between each client and the server

public sealed class MeterServerClient : IAsyncDisposable // Disposable so we can close SignalR connection when done with it
{
    private readonly HubConnection _connection;

    // Makes it wait for previous readings to load before starting to send normal readings
    private readonly TaskCompletionSource _readingsLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public MeterServerClient(
        Uri serverUri,
        string meterId,
        string accessToken)
    {
        Uri hubUri = new(serverUri, $"/api/v1/meters/{meterId}"); // Connects to its own SignalR endpoint using its individual meterID

        _connection = new HubConnectionBuilder().WithUrl(
            hubUri,
            options =>
            {
                options.AccessTokenProvider = // Gives the access token which the server uses to authorise the connection
                    () => Task.FromResult<string?>(accessToken);
            }).WithAutomaticReconnect().Build();// Tries to reconnect if communication issues 

        // Handlers for messages and events that can be pushed from the server to clients

        // When the server recalculates the meters bill
        _connection.On<Bill>("BillUpdated", bill =>
        {
            Console.WriteLine($"Bill: {bill.Units} kWh at {bill.UnitPrice}/kWh = {bill.Total}");
        });

        // When server sends an alert related to grid
        _connection.On<GridAlert>("GridAlert", alert =>
        {
            Console.WriteLine($"Grid Alert: {alert.Message}");
        });

        // When the server refuses a reading submitted by this meter
        _connection.On<string>("ReadingRejected", reason =>
        {
            Console.WriteLine($"Reading rejected: {reason}");
        });

        // Used to send the meters old readings back to the client
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
            _readingsLoaded.TrySetResult(); // Tells ConnectAsync that it has received all historical readings
        });

    }

    // Opens the SignalR connection and waits for historical readings
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

    // Sends a new meter reading to the server through Async
    public async Task SendReadingAsync(Reading reading)
    {
        await _connection.InvokeAsync(
            "SubmitReading", reading);
    }

    // Gets rid of SignalR connection when client doesn't need it anymre
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
