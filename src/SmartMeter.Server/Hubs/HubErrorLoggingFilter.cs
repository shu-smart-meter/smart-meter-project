using Microsoft.AspNetCore.SignalR;

namespace SmartMeter.Server.Hubs;

public sealed class HubErrorLoggingFilter : IHubFilter
{
    private readonly ILogger<HubErrorLoggingFilter> _logger;

    public HubErrorLoggingFilter(ILogger<HubErrorLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Meter {MeterId} failed to invoke {Method}",
                invocationContext.Context.GetHttpContext()?.Request.RouteValues["meterId"],
                invocationContext.HubMethodName);
            throw;
        }
    }
}
