using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Products;

/// <summary>
/// Input DTO for setting product attribute values
/// </summary>
public class ProductAttributeValueInputDto
{
    /// <summary>
    /// The attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Value for Text type attributes
    /// </summary>
    public string? TextValue { get; set; }

    /// <summary>
    /// Value for Number type attributes
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// Selected value ID for Select type attributes
    /// </summary>
    public Guid? SelectValueId { get; set; }

    /// <summary>
    /// Selected value IDs for MultiSelect type attributes
    /// </summary>
    public List<Guid>? MultiSelectValueIds { get; set; }

    /// <summary>
    /// Value for Boolean type attributes
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Value for Date type attributes
    /// </summary>
    public DateTime? DateValue { get; set; }
}

/// <summary>
/// Output DTO for product attribute values (includes attribute metadata)
/// </summary>
public class ProductAttributeValueOutputDto
{
    /// <summary>
    /// The attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Attribute code (for reference)
    /// </summary>
    public string AttributeCode { get; set; } = string.Empty;

    /// <summary>
    /// Attribute display name
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Attribute type (Text, Number, Select, etc.)
    /// </summary>
    public AttributeType AttributeType { get; set; }

    /// <summary>
    /// Unit of measurement (if applicable)
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Value for Text type attributes
    /// </summary>
    public string? TextValue { get; set; }

    /// <summary>
    /// Value for Number type attributes
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// Selected value ID for Select type attributes
    /// </summary>
    public Guid? SelectValueId { get; set; }

    /// <summary>
    /// Display text of selected value for Select type
    /// </summary>
    public string? SelectValueText { get; set; }

    /// <summary>
    /// Selected value IDs for MultiSelect type attributes
    /// </summary>
    public List<Guid>? MultiSelectValueIds { get; set; }

    /// <summary>
    /// Display texts of selected values for MultiSelect type
    /// </summary>
    public List<string>? MultiSelectValueTexts { get; set; }

    /// <summary>
    /// Value for Boolean type attributes
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Value for Date type attributes
    /// </summary>
    public DateTime? DateValue { get; set; }
}
