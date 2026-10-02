using Microsoft.AspNetCore.Mvc;

using SmartMeter.Server.Models;

namespace SmartMeter.Server.Controllers;

[Route("")]
public sealed class RootController : BaseApiController
{
    [HttpGet]
    public GreetingResponse Get()
    {
        return new GreetingResponse { Message = "Hello World!" };
    }
}
