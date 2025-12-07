namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Summary DTO for API key list views
/// </summary>
public class ApiKeyListDto
{
    public Guid Id { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int RateLimitPerMinute { get; set; }
    public int PermissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
