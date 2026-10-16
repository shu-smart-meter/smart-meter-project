using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR.Client;
using SmartMeter.Model;


public sealed class MeterServerClient : IAsyncDisposable // Disposable so we can close SignalR connection when done with it
{
    private readonly HubConnection _connection;

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

    }

    public async Task ConnectAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _connection.StartAsync(cancellationToken);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Connected to server");
        Console.ResetColor();
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
