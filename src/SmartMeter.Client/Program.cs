using SmartMeter.Client.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var generator = new ReadingGenerator(100.0);

        string meterId = args.Length > 0 ? args[0] : "Meter-001";

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
            Console.WriteLine($"[{reading.Timestamp}] Meter ID: {reading.MeterId}, Reading: {reading.Reading:F2} kWh");
            Console.ResetColor();

            int delaySeconds = generator.GetNextDelaySeconds();
            Console.WriteLine($"Next reading in {delaySeconds} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}