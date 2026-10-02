using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

using SmartMeter.Server.Responses;

namespace SmartMeter.Server.Middleware;

// handles error responses that do not reach a controller, eg an unmatched route
public static class StatusCodeResponseWriter
{
    public static Task WriteAsync(StatusCodeContext context)
    {
        var httpContext = context.HttpContext;
        var status = httpContext.Response.StatusCode;
        var message = status == StatusCodes.Status404NotFound
            ? $"No route matches {httpContext.Request.Method} {httpContext.Request.Path}"
            : ReasonPhrases.GetReasonPhrase(status);

        return httpContext.Response.WriteAsJsonAsync(ApiResponse.Failure(status, message));
    }
}
