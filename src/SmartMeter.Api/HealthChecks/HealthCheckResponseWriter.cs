using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using SmartMeter.Api.Models;

namespace SmartMeter.Api.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        var response = new HealthResponse { Status = report.Status.ToString() };

        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
