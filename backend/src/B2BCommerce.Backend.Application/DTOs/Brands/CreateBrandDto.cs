namespace B2BCommerce.Backend.Application.DTOs.Brands;

/// <summary>
/// Data transfer object for creating a new brand
/// </summary>
public class CreateBrandDto
{
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
    /// Website URL
    /// </summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Whether brand is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
