using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SmartMeter.Api.Responses;

namespace SmartMeter.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T response)
    {
        var envelope = ApiResponse.Success(StatusCodes.Status200OK, response);

        return StatusCode(envelope.Status, envelope);
    }
}
