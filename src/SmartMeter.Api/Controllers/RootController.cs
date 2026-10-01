using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Models;

namespace SmartMeter.Api.Controllers;

[Route("")]
public sealed class RootController : BaseApiController
{
    [HttpGet]
    public GreetingResponse Get()
    {
        return new GreetingResponse { Message = "Hello World!" };
    }
}
