namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for API key usage log entries
/// </summary>
public class ApiKeyUsageLogDto
{
    public long Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime RequestTimestamp { get; set; }
    public int ResponseStatusCode { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}
