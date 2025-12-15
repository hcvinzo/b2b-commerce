namespace B2BCommerce.Backend.Application.DTOs.ProductTypes;

/// <summary>
/// DTO for creating a new product type
/// </summary>
public class CreateProductTypeDto
{
    /// <summary>
    /// Unique code (e.g., "memory_card", "ssd")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Admin description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initial attribute assignments
    /// </summary>
    public List<AddAttributeToProductTypeDto>? Attributes { get; set; }
}

/// <summary>
/// DTO for adding an attribute to a product type
/// </summary>
public class AddAttributeToProductTypeDto
{
    /// <summary>
    /// Attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Whether this attribute is required for this product type
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Display order within this product type
    /// </summary>
    public int DisplayOrder { get; set; }
}
