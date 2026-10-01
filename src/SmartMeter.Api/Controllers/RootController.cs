using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Models;
using SmartMeter.Api.Responses;

namespace SmartMeter.Api.Controllers;

[Route("")]
public sealed class RootController : BaseApiController
{
    [HttpGet]
    public ActionResult<ApiResponse<GreetingResponse>> Get()
    {
        return Success(new GreetingResponse { Message = "Hello World!" });
    }
}
