namespace B2BCommerce.Backend.Application.DTOs.Brands;

/// <summary>
/// Brand data transfer object for output
/// </summary>
public class BrandDto
{
    /// <summary>
    /// Brand identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Brand name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brand description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Logo URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Whether brand is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// External system ID (LOGO ERP primary key)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// External system code (optional reference)
    /// </summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// Date of last synchronization with external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
