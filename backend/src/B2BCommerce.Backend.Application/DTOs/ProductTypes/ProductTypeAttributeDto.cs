using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.ProductTypes;

/// <summary>
/// Product type attribute data transfer object
/// </summary>
public class ProductTypeAttributeDto
{
    /// <summary>
    /// Product type attribute ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Attribute external ID (for Integration API)
    /// </summary>
    public string? AttributeExternalId { get; set; }

    /// <summary>
    /// Attribute code
    /// </summary>
    public string AttributeCode { get; set; } = string.Empty;

    /// <summary>
    /// Attribute name
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Attribute type
    /// </summary>
    public AttributeType AttributeType { get; set; }

    /// <summary>
    /// Unit of measurement
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this attribute is required for this product type
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Display order within this product type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Predefined values (for Select/MultiSelect types)
    /// </summary>
    public List<AttributeValueDto>? PredefinedValues { get; set; }
}
