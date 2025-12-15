namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Brand data transfer object for API responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guid is never exposed.
/// </summary>
public class BrandDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Brand list item for paginated responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guid is never exposed.
/// </summary>
public class BrandListDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }

    public DateTime? LastSyncedAt { get; set; }
}
