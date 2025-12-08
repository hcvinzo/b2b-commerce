namespace B2BCommerce.Backend.IntegrationAPI.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Error(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };
}

/// <summary>
/// Standard API response wrapper with data
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public new static ApiResponse<T> Error(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };
}

/// <summary>
/// Paged API response for list endpoints
/// </summary>
public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    public PaginationMeta? Pagination { get; set; }

    public static PagedApiResponse<T> Ok(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string? message = null)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            }
        };
    }
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMeta
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
