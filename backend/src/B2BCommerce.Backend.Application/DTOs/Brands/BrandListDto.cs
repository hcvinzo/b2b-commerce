namespace B2BCommerce.Backend.Application.DTOs.Brands;

/// <summary>
/// Simplified brand data transfer object for list views
/// </summary>
public class BrandListDto
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
    public string? Description { get; set; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Whether brand is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of products in this brand
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
