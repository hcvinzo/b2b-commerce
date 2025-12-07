namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for creating a new API key
/// </summary>
public class CreateApiKeyDto
{
    public Guid ApiClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RateLimitPerMinute { get; set; } = 500;
    public DateTime? ExpiresAt { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<string> IpWhitelist { get; set; } = new();
}
