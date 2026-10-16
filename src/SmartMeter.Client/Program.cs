using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using SmartMeter.Client.Services;
using SmartMeter.Model;

internal static class Program
{
    private static async Task Main(string[] args)
    {

        var generator = new ReadingGenerator();

        string meterId = args.Length > 0 ? args[0] : "Meter-001";


        Uri serverUri = new("http://localhost:5080");
        using var httpClient = new HttpClient();

        var authClient = new MeterAuthClient(serverUri, httpClient);

        string clientSecret = "smart-meter-secret";

        TokenResponse token = await authClient.RequestTokenAsync(meterId, clientSecret);
        await using var serverClient = new MeterServerClient(serverUri, meterId, token.AccessToken);

        await serverClient.ConnectAsync();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================");
        Console.WriteLine(" SMART METER CLIENT");
        Console.WriteLine("======================================");
        Console.WriteLine($"Meter ID: {meterId}");
        Console.WriteLine();
        Console.ResetColor();

        while (true)
        {
            var reading = generator.GenerateReading(meterId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{reading.Timestamp}] Meter ID: {reading.MeterId}, Consumption: {reading.KilowattHours:F2} kWh");
            Console.ResetColor();

            await serverClient.SendReadingAsync(reading);
            Console.WriteLine("Reading sent to server");
            int delaySeconds = generator.GetNextDelaySeconds();
            Console.WriteLine($"Next reading in {delaySeconds} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}
