namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// Attribute value data transfer object for output
/// </summary>
public class AttributeValueDto
{
    /// <summary>
    /// Attribute value identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Internal value (e.g., "256", "red")
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// User-facing display text (e.g., "256 GB", "Red")
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Display order in dropdowns/lists
    /// </summary>
    public int DisplayOrder { get; set; }
}
