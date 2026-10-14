using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SmartMeter.Server.Filters;
using SmartMeter.Server.HealthChecks;
using SmartMeter.Server.Hubs;
using SmartMeter.Server.Middleware;
using SmartMeter.Server.Services;
using SmartMeter.Server.Storage;

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
        // useexceptionhandler needs problem details services, even with a custom handler
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();

        // share one store across meters; appsettings.json sets the default path
        services.AddSingleton<IReadingStore>(
            provider => new JsonReadingStore(GetStoragePath(provider)));

        // these services keep no state between calls
        services.AddSingleton<IVerificationService, VerificationService>();
        services.AddSingleton<IBillCalculationStrategy, FlatRateBillCalculationStrategy>();
        services.AddSingleton<IBillingService, BillingService>();

        // keep alerts for the read endpoints and send them to connected meters
        services.AddSingleton<IAlertNotifier, HubAlertNotifier>();
        services.AddSingleton<IBillNotifier, HubBillNotifier>();
        services.AddSingleton<IGridAlertService, GridAlertService>();

        // credentials come from environment variables
        services.AddSingleton<AuthConfig>(
            provider => AuthConfig.From(provider.GetRequiredService<IConfiguration>()));
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IAuthenticationExchange, AuthenticationExchange>();

        // bearer tokens are the one way to authenticate; http and the hub both use it
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = MeterTokenOptions.DefaultScheme;
            options.DefaultChallengeScheme = MeterTokenOptions.DefaultScheme;
            options.DefaultForbidScheme = MeterTokenOptions.DefaultScheme;
        }).AddScheme<MeterTokenOptions, MeterTokenAuthenticationHandler>(
            MeterTokenOptions.DefaultScheme,
            options => { });

        services.AddAuthorization(options =>
        {
            // every hub request below this policy needs the meter role, and
            // the token name must equal the meter the route names
            options.AddPolicy("MeterOwnResource", policy =>
            {
                policy.RequireRole(new string[] { "meter" });
                policy.RequireAssertion(context =>
                {
                    if (context.Resource is not HttpContext http)
                    {
                        return Task.FromResult(false);
                    }

                    string? name = context.User?.Identity?.Name;
                    string? route = http.Request.RouteValues["meterId"]?.ToString();

                    return Task.FromResult(name is not null && name.Equals(route, StringComparison.Ordinal));
                });
            });
        });

        services.AddSignalR().AddHubOptions<MeterHub>(options => options.AddFilter(typeof(HubErrorLoggingFilter)));
    }

    public void Configure(WebApplication app)
    {
        app.UseExceptionHandler();
        // return JSON for errors when the client requests another content type
        app.UseStatusCodePages(StatusCodeResponseWriter.WriteAsync);
        // every authenticated route reads its identity from the bearer token
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
        });

        // meters connect to their own resource route; identity comes from
        // the token claim and must match the route, never the url alone
        app.MapHub<MeterHub>("/api/v1/meters/{meterId}", options =>
        {
            // close a connection whose token has expired instead of keeping
            // it alive on stale credentials
            options.CloseOnAuthenticationExpiration = true;
        }).RequireAuthorization("MeterOwnResource");
    }

    // read the storage path from config, or use a file beside the application
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
