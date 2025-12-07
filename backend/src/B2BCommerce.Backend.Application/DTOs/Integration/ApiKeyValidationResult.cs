namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Result of API key validation for authentication
/// </summary>
public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public Guid? ApiKeyId { get; set; }
    public Guid? ApiClientId { get; set; }
    public string? ClientName { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int RateLimitPerMinute { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ApiKeyValidationResult Success(
        Guid apiKeyId,
        Guid apiClientId,
        string clientName,
        IEnumerable<string> permissions,
        int rateLimitPerMinute)
    {
        return new ApiKeyValidationResult
        {
            IsValid = true,
            ApiKeyId = apiKeyId,
            ApiClientId = apiClientId,
            ClientName = clientName,
            Permissions = permissions.ToList(),
            RateLimitPerMinute = rateLimitPerMinute
        };
    }

    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    public static ApiKeyValidationResult Failed(string errorCode, string message)
    {
        return new ApiKeyValidationResult
        {
            IsValid = false,
            ErrorCode = errorCode,
            ErrorMessage = message
        };
    }
}
