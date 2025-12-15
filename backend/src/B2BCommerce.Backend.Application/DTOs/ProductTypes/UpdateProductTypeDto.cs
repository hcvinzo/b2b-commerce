namespace B2BCommerce.Backend.Application.DTOs.ProductTypes;

/// <summary>
/// DTO for updating a product type
/// </summary>
public class UpdateProductTypeDto
{
    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Admin description
    /// </summary>
    public string? Description { get; set; }
}
