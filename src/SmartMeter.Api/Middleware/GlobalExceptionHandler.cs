using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SmartMeter.Api.Responses;

namespace SmartMeter.Api.Middleware;

// catches anything a controller throws and sends it back in the same envelope as everything else
// writes the json itself instead of going through problem details, so the accept header cant change the format
// why: https://learn.microsoft.com/aspnet/core/fundamentals/error-handling#iexceptionhandler
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var envelope = ApiResponse.Failure(
            StatusCodes.Status500InternalServerError,
            ApiErrorCode.InternalError,
            "An unexpected error occurred");

        httpContext.Response.StatusCode = envelope.Status;
        await httpContext.Response.WriteAsJsonAsync(envelope, cancellationToken);

        return true;
    }
}
