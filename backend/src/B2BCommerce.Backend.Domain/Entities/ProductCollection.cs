using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction entity for products in a collection (manual collections)
/// </summary>
public class ProductCollection : BaseEntity
{
    /// <summary>
    /// FK to the Collection
    /// </summary>
    public Guid CollectionId { get; private set; }

    /// <summary>
    /// FK to the Product
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Display order of this product within the collection
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Whether this product is featured/highlighted in the collection
    /// </summary>
    public bool IsFeatured { get; private set; }

    // Navigation properties
    public Collection Collection { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private ProductCollection() // For EF Core
    {
    }

    /// <summary>
    /// Creates a new ProductCollection instance
    /// </summary>
    public static ProductCollection Create(
        Guid collectionId,
        Guid productId,
        int displayOrder = 0,
        bool isFeatured = false)
    {
        if (collectionId == Guid.Empty)
        {
            throw new DomainException("CollectionId is required");
        }

        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required");
        }

        return new ProductCollection
        {
            CollectionId = collectionId,
            ProductId = productId,
            DisplayOrder = displayOrder,
            IsFeatured = isFeatured
        };
    }

    /// <summary>
    /// Sets this product as featured in the collection
    /// </summary>
    public void SetAsFeatured()
    {
        IsFeatured = true;
    }

    /// <summary>
    /// Removes featured status
    /// </summary>
    public void RemoveFeaturedStatus()
    {
        IsFeatured = false;
    }

    /// <summary>
    /// Updates the display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
