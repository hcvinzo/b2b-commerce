namespace B2BCommerce.Backend.Application.Common;

/// <summary>
/// Result pattern for operation responses
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public string? ErrorCode { get; protected set; }
    public Dictionary<string, string[]>? ValidationErrors { get; protected set; }

    protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result Success() => new Result(true);

    public static Result Failure(string errorMessage, string? errorCode = null)
        => new Result(false, errorMessage, errorCode);

    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors)
        => new Result(false, "Validation failed", "VALIDATION_ERROR") { ValidationErrors = validationErrors };
}

/// <summary>
/// Result pattern with data payload
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new Result<T>(true, data);

    public new static Result<T> Failure(string errorMessage, string? errorCode = null)
        => new Result<T>(false, default, errorMessage, errorCode);

    public new static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors)
        => new Result<T>(false, default, "Validation failed", "VALIDATION_ERROR") { ValidationErrors = validationErrors };
}
