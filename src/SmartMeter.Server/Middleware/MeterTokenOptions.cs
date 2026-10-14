using Microsoft.AspNetCore.Authentication;

namespace SmartMeter.Server.Middleware;

public sealed class MeterTokenOptions : AuthenticationSchemeOptions
{
    public static readonly string DefaultScheme = "MeterToken";
}
