using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Filters;
using SmartMeter.Api.HealthChecks;
using SmartMeter.Api.Hubs;
using SmartMeter.Api.Middleware;
using SmartMeter.Implementations;
using SmartMeter.Model;

namespace SmartMeter.Server;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add<ApiResponseResultFilter>();
        });
        // nothing writes problem details anymore but UseExceptionHandler() wont start without it
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();

        // one shared store so every meter reads and writes the same file
        // the path comes from config; appsettings.json sets a default
        services.AddSingleton<IReadingStore>(
            provider => new JsonReadingStore(GetStoragePath(provider)));

        // both are stateless, so one instance is enough
        services.AddSingleton<IVerificationService, VerificationService>();
        services.AddSingleton<IBillingService, BillingService>();

        services.AddSignalR();
    }

    public void Configure(WebApplication app)
    {
        app.UseExceptionHandler();
        // always json; the default handler sends plain text if the client asks for text
        app.UseStatusCodePages(StatusCodeResponseWriter.WriteAsync);

        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
        });

        // each meter gets its own address, and that address is its identity
        app.MapHub<MeterHub>("/api/v1/meters/{meterId}");
    }

    // resolves the storage path from config, falling back to a file next to the app
    private static string GetStoragePath(IServiceProvider provider)
    {
        IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
        string? configured = configuration["Storage:Path"];

        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.Combine(AppContext.BaseDirectory, "readings.json");
        }

        return Path.GetFullPath(configured);
    }
}
