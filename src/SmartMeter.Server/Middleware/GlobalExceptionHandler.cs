using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SmartMeter.Server.Responses;

namespace SmartMeter.Server.Middleware;

// logs unhandled exceptions and returns them in the API response envelope
// writes JSON directly so the response format does not depend on the Accept header
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
