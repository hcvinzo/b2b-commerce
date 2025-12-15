namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Brand data transfer object for API responses
/// </summary>
public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; }

    // External entity fields
    public string? ExtId { get; set; }
    public string? ExtCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Brand list item for paginated responses
/// </summary>
public class BrandListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }

    // External entity fields
    public string? ExtId { get; set; }
    public string? ExtCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
