namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// DTO for adding a predefined value to an existing attribute definition
/// </summary>
public class AddAttributeValueDto
{
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
