namespace SmartMeter.Api.Responses;

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(int status, T response)
    {
        return new ApiResponse<T> { Ok = true, Status = status, Response = response };
    }
}
