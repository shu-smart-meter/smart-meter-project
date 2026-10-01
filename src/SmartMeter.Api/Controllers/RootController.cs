using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Models;

namespace SmartMeter.Api.Controllers;

[ApiController]
[Route("")]
public sealed class RootController : ControllerBase
{
    [HttpGet]
    public GreetingResponse Get()
    {
        return new GreetingResponse { Message = "Hello World!" };
    }
}
