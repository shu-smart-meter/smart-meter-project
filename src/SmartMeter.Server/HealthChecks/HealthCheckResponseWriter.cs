using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using SmartMeter.Api.Models;
using SmartMeter.Api.Responses;

namespace SmartMeter.Api.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        var status = httpContext.Response.StatusCode;

        if (status >= StatusCodes.Status400BadRequest)
        {
            return httpContext.Response.WriteAsJsonAsync(ApiResponse.Failure(status, $"Server is {report.Status}"));
        }

        var response = new HealthResponse { Status = report.Status.ToString() };

        return httpContext.Response.WriteAsJsonAsync(ApiResponse.Success(status, response));
    }
}
