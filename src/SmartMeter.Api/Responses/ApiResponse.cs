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
}
