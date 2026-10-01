using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

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
        });
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
    }

    public void Configure(WebApplication app)
    {
        app.UseExceptionHandler();
        // always json; the default handler sends plain text if the client asks for text
        app.UseStatusCodePages(context =>
        {
            var httpContext = context.HttpContext;

            return Results.Problem(statusCode: httpContext.Response.StatusCode).ExecuteAsync(httpContext);
        });

        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
        });
    }
}
