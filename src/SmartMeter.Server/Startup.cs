using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Middleware;

namespace SmartMeter.Server;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
        });
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
    }

    public void Configure(WebApplication app)
    {
        app.UseExceptionHandler();

        app.MapControllers();
        app.MapHealthChecks("/health");
    }
}
