using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// DTO for setting a product attribute value
/// </summary>
public class SetProductAttributeValueDto
{
    /// <summary>
    /// Attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

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
}
