namespace B2BCommerce.Backend.Application.DTOs.Products;

/// <summary>
/// Product category assignment data transfer object
/// </summary>
public class ProductCategoryDto
{
    /// <summary>
    /// Product category assignment identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Category slug
    /// </summary>
    public string CategorySlug { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the primary category for the product
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Display order of the product within this category
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for adding a product to a category
/// </summary>
public class AddProductToCategoryDto
{
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Whether this should be the primary category
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Display order of the product within this category
    /// </summary>
    public int DisplayOrder { get; set; }
}
