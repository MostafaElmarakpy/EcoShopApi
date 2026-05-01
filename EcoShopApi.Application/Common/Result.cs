namespace EcoShopApi.Application.Common;

/// <summary>
/// Represents the result of an operation, replacing exception-driven flow.
/// Enables better error handling and composition.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public ErrorCode ErrorCode { get; set; }

    public static Result<T> Success(T data) =>
        new() { IsSuccess = true, Data = data, ErrorCode = ErrorCode.None };

    public static Result<T> Failure(string message, ErrorCode code = ErrorCode.InternalError) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}

/// <summary>
/// Non-generic Result for operations that don't return data (e.g., DELETE).
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public ErrorCode ErrorCode { get; set; }

    public static Result Success() =>
        new() { IsSuccess = true, ErrorCode = ErrorCode.None };

    public static Result Failure(string message, ErrorCode code = ErrorCode.InternalError) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}

public enum ErrorCode
{
    None = 0,
    ProductNotFound = 1,
    InvalidProductData = 2,
    CategoryNotFound = 3,
    UserNotFound = 4,
    InvalidImageFile = 5,
    DuplicateProduct = 6,
    InternalError = 500
}
