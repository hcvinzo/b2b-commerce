namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for API key usage statistics
/// </summary>
public class ApiKeyUsageStatsDto
{
    public Guid ApiKeyId { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public DateTime? FirstUsedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public Dictionary<string, long> RequestsByEndpoint { get; set; } = new();
    public Dictionary<int, long> RequestsByStatusCode { get; set; } = new();
}
