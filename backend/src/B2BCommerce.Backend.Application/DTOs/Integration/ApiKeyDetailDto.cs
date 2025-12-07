namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Detailed DTO for API key with all information
/// </summary>
public class ApiKeyDetailDto
{
    public Guid Id { get; set; }
    public Guid ApiClientId { get; set; }
    public string ApiClientName { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? LastUsedIp { get; set; }
    public int RateLimitPerMinute { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string? RevocationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<IpWhitelistDto> IpWhitelist { get; set; } = new();
}
