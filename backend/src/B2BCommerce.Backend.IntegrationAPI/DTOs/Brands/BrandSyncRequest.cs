using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Request DTO for syncing a brand from external system (LOGO ERP).
/// Id = ExternalId (string) - the primary upsert key.
/// Code = ExternalCode (string) - optional secondary reference.
/// </summary>
public class BrandSyncRequest
{
    /// <summary>
    /// External ID (PRIMARY upsert key - required for new brands).
    /// This is the ID from the source system (LOGO ERP).
    /// </summary>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// External code (OPTIONAL secondary reference)
    /// </summary>
    [StringLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Brand name (required)
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Brand description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Whether brand is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for bulk syncing brands from external system.
/// </summary>
public class BulkBrandSyncRequest
{
    /// <summary>
    /// List of brands to sync
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<BrandSyncRequest> Brands { get; set; } = new();
}
