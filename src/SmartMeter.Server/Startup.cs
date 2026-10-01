using SmartMeter.Api.Middleware;

namespace SmartMeter.Server;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
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
