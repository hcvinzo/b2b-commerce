namespace B2BCommerce.Backend.Application.DTOs.ProductTypes;

/// <summary>
/// Product type list item data transfer object
/// </summary>
public class ProductTypeListDto
{
    /// <summary>
    /// Product type identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code
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
    /// Whether new products can use this type
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of attributes assigned
    /// </summary>
    public int AttributeCount { get; set; }

    /// <summary>
    /// Number of products using this type
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
