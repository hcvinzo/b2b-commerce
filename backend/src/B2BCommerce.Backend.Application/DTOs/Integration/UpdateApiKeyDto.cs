namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for updating an existing API key (name, rate limit, expiration only)
/// </summary>
public class UpdateApiKeyDto
{
    public string Name { get; set; } = string.Empty;
    public int RateLimitPerMinute { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
