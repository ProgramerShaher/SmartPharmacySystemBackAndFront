using System.Text.Json.Serialization;

namespace SmartPharmacySystem.Application.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public ApiResponse() { }

    public ApiResponse(T data, string? message = null)
    {
        Success = true;
        Message = message;
        Data = data;
        StatusCode = 200;
    }

    public ApiResponse(string message)
    {
        Success = false;
        Message = message;
        Data = default;
        StatusCode = 400;
    }

    public static ApiResponse<T> Succeeded(T data, string message, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Failed(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode
        };
    }
}
