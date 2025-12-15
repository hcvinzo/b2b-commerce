using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// Product attribute value data transfer object for output
/// </summary>
public class ProductAttributeValueDto
{
    /// <summary>
    /// Product attribute value identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

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
    /// Text value (for Text type)
    /// </summary>
    public string? TextValue { get; set; }

    /// <summary>
    /// Numeric value (for Number type)
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// Selected value ID (for Select type)
    /// </summary>
    public Guid? AttributeValueId { get; set; }

    /// <summary>
    /// Selected value display text (for Select type)
    /// </summary>
    public string? SelectedValueText { get; set; }

    /// <summary>
    /// Boolean value (for Boolean type)
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Date value (for Date type)
    /// </summary>
    public DateTime? DateValue { get; set; }

    /// <summary>
    /// Selected value IDs (for MultiSelect type)
    /// </summary>
    public List<Guid>? MultiSelectValueIds { get; set; }

    /// <summary>
    /// Selected values display texts (for MultiSelect type)
    /// </summary>
    public List<string>? MultiSelectValueTexts { get; set; }

    /// <summary>
    /// Formatted display value (for UI display)
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;
}
