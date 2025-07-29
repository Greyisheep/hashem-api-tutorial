namespace TaskFlow.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
    public string Version { get; set; } = "1.0.0";

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Message = message
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static new ApiResponse SuccessResponse(object data, string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static new ApiResponse ErrorResponse(string error, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = error,
            Message = message
        };
    }
} 