using Microsoft.AspNetCore.Mvc;

namespace SmartMeter.Api.Controllers;

[ApiController]
[Route("")]
public sealed class RootController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "Hello World!";
    }
}
