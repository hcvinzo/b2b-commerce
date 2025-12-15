using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction entity allowing products to belong to multiple categories
/// </summary>
public class ProductCategory : BaseEntity
{
    /// <summary>
    /// FK to the Product
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// FK to the Category
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Whether this is the primary category for the product (used for canonical URL)
    /// Only one category per product can be primary
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Display order of this product within this category
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    private ProductCategory() // For EF Core
    {
    }

    /// <summary>
    /// Creates a new ProductCategory instance
    /// </summary>
    internal static ProductCategory Create(
        Guid productId,
        Guid categoryId,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("CategoryId is required");
        }

        return new ProductCategory
        {
            ProductId = productId,
            CategoryId = categoryId,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Sets this as the primary category
    /// </summary>
    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    /// <summary>
    /// Removes primary status
    /// </summary>
    public void RemovePrimaryStatus()
    {
        IsPrimary = false;
    }

    /// <summary>
    /// Updates the display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
