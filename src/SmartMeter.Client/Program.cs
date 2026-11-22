
using SmartMeter.Client.Services;
using SmartMeter.Model;

internal static class Program
{
    private static async Task Main(string[] args)
    {

        var generator = new ReadingGenerator();

        string meterId = args.Length > 0 ? args[0] : "Meter-001";


        string serverUrl = Environment.GetEnvironmentVariable("SMART_METER_SERVER_URL") ?? "http://localhost:5080";

        string clientSecret = Environment.GetEnvironmentVariable("SMART_METER_CLIENT_SECRET") ?? "local-development-client-secret";

        Uri serverUri = new(serverUrl);

        using var httpClient = new HttpClient();

        var authClient = new MeterAuthClient(serverUri, httpClient);

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

            try
            {
                await serverClient.SendReadingAsync(reading);
                Console.WriteLine("Reading sent to server");
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to send reading. The server connection is unavailable");
                Console.ResetColor();
            }

            int delaySeconds = generator.GetNextDelaySeconds();
            Console.WriteLine($"Next reading in {delaySeconds} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}
