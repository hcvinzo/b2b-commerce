namespace B2BCommerce.Backend.Domain.Entities.Integration;

/// <summary>
/// Represents a usage log entry for API key requests.
/// Note: This entity does not inherit from BaseEntity due to high-volume nature.
/// Uses long for ID to handle large number of records.
/// </summary>
public class ApiKeyUsageLog
{
    public long Id { get; set; }
    public Guid ApiKeyId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime RequestTimestamp { get; set; }
    public int ResponseStatusCode { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation
    public ApiKey ApiKey { get; set; } = null!;

    /// <summary>
    /// Creates a new usage log entry
    /// </summary>
    public static ApiKeyUsageLog Create(
        Guid apiKeyId,
        string endpoint,
        string httpMethod,
        string ipAddress,
        string? userAgent,
        int responseStatusCode,
        int responseTimeMs,
        string? errorMessage = null)
    {
        return new ApiKeyUsageLog
        {
            ApiKeyId = apiKeyId,
            Endpoint = endpoint,
            HttpMethod = httpMethod,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RequestTimestamp = DateTime.UtcNow,
            ResponseStatusCode = responseStatusCode,
            ResponseTimeMs = responseTimeMs,
            ErrorMessage = errorMessage
        };
    }
}
