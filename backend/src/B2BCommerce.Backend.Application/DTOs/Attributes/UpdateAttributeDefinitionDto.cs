namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// DTO for updating an attribute definition
/// </summary>
public class UpdateAttributeDefinitionDto
{
    /// <summary>
    /// Display name in Turkish
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name in English
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Unit of measurement (e.g., "GB", "MB/s")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this attribute should appear in product filters
    /// </summary>
    public bool IsFilterable { get; set; }

    /// <summary>
    /// Default required status
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; }

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; set; }
}
