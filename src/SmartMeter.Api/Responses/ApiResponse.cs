using Microsoft.AspNetCore.Http;

namespace SmartMeter.Api.Responses;

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(int status, T response)
    {
        return new ApiResponse<T> { Ok = true, Status = status, Response = response };
    }

    public static ApiResponse<ApiError> Failure(int status, ApiErrorCode error, string message)
    {
        return new ApiResponse<ApiError>
        {
            Ok = false,
            Status = status,
            Response = new ApiError { Error = error, Message = message },
        };
    }

    public static ApiResponse<ApiError> Failure(int status, string message)
    {
        return Failure(status, ErrorCodeFor(status), message);
    }

    private static ApiErrorCode ErrorCodeFor(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => ApiErrorCode.BadRequest,
            StatusCodes.Status401Unauthorized => ApiErrorCode.Unauthorized,
            StatusCodes.Status403Forbidden => ApiErrorCode.Forbidden,
            StatusCodes.Status404NotFound => ApiErrorCode.NotFound,
            StatusCodes.Status405MethodNotAllowed => ApiErrorCode.MethodNotAllowed,
            StatusCodes.Status415UnsupportedMediaType => ApiErrorCode.UnsupportedMediaType,
            StatusCodes.Status503ServiceUnavailable => ApiErrorCode.ServiceUnavailable,
            >= 500 => ApiErrorCode.InternalError,
            _ => ApiErrorCode.ClientError,
        };
    }
}
