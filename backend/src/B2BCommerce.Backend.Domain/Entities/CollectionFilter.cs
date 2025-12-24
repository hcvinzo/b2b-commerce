using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Stores filter criteria for dynamic collections.
/// One-to-one relationship with Collection.
/// </summary>
public class CollectionFilter : BaseEntity
{
    /// <summary>
    /// FK to the Collection (also serves as PK)
    /// </summary>
    public Guid CollectionId { get; private set; }

    /// <summary>
    /// Category IDs to include products from
    /// </summary>
    public List<Guid> CategoryIds { get; private set; } = new();

    /// <summary>
    /// Brand IDs to include products from
    /// </summary>
    public List<Guid> BrandIds { get; private set; } = new();

    /// <summary>
    /// Product Type IDs to include products from
    /// </summary>
    public List<Guid> ProductTypeIds { get; private set; } = new();

    /// <summary>
    /// Minimum price filter (inclusive)
    /// </summary>
    public decimal? MinPrice { get; private set; }

    /// <summary>
    /// Maximum price filter (inclusive)
    /// </summary>
    public decimal? MaxPrice { get; private set; }

    // Navigation property
    public Collection Collection { get; private set; } = null!;

    private CollectionFilter() // For EF Core
    {
    }

    /// <summary>
    /// Creates a new CollectionFilter instance
    /// </summary>
    public static CollectionFilter Create(
        Guid collectionId,
        List<Guid>? categoryIds = null,
        List<Guid>? brandIds = null,
        List<Guid>? productTypeIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        if (collectionId == Guid.Empty)
        {
            throw new DomainException("CollectionId is required");
        }

        if (minPrice.HasValue && maxPrice.HasValue && maxPrice < minPrice)
        {
            throw new DomainException("Max price cannot be less than min price");
        }

        return new CollectionFilter
        {
            CollectionId = collectionId,
            CategoryIds = categoryIds ?? new List<Guid>(),
            BrandIds = brandIds ?? new List<Guid>(),
            ProductTypeIds = productTypeIds ?? new List<Guid>(),
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
    }

    /// <summary>
    /// Updates the filter criteria
    /// </summary>
    public void Update(
        List<Guid>? categoryIds,
        List<Guid>? brandIds,
        List<Guid>? productTypeIds,
        decimal? minPrice,
        decimal? maxPrice)
    {
        if (minPrice.HasValue && maxPrice.HasValue && maxPrice < minPrice)
        {
            throw new DomainException("Max price cannot be less than min price");
        }

        CategoryIds = categoryIds ?? new List<Guid>();
        BrandIds = brandIds ?? new List<Guid>();
        ProductTypeIds = productTypeIds ?? new List<Guid>();
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }

    /// <summary>
    /// Checks if the filter has any criteria set
    /// </summary>
    public bool HasAnyCriteria()
    {
        return CategoryIds.Count > 0 ||
               BrandIds.Count > 0 ||
               ProductTypeIds.Count > 0 ||
               MinPrice.HasValue ||
               MaxPrice.HasValue;
    }

    /// <summary>
    /// Clears all filter criteria
    /// </summary>
    public void Clear()
    {
        CategoryIds.Clear();
        BrandIds.Clear();
        ProductTypeIds.Clear();
        MinPrice = null;
        MaxPrice = null;
    }
}
