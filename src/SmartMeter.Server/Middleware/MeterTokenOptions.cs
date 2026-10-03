using Microsoft.AspNetCore.Authentication;

namespace SmartMeter.Server.Middleware;

// options for the meter token authentication scheme
public sealed class MeterTokenOptions : AuthenticationSchemeOptions
{
    public static readonly string DefaultScheme = "MeterToken";
}