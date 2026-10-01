using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Filters;
using SmartMeter.Api.HealthChecks;
using SmartMeter.Api.Middleware;

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
    }
}
