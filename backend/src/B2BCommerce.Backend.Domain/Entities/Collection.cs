using System.Text.RegularExpressions;
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Collection entity for virtual product groupings (e.g., "Summer Sale", "Black Friday").
/// Supports both manual (admin-selected) and dynamic (filter-based) product collections.
/// </summary>
public class Collection : ExternalEntity, IAggregateRoot
{
    /// <summary>
    /// Maximum number of products allowed in a manual collection
    /// </summary>
    public const int MaxProductsPerManualCollection = 50;

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public CollectionType Type { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Computed property: collection is currently active based on IsActive flag and date range
    /// </summary>
    public bool IsCurrentlyActive => IsActive &&
        (StartDate is null || StartDate <= DateTime.UtcNow) &&
        (EndDate is null || EndDate >= DateTime.UtcNow);

    // Navigation properties
    private readonly List<ProductCollection> _productCollections = new();
    public IReadOnlyCollection<ProductCollection> ProductCollections => _productCollections.AsReadOnly();

    // One-to-one relationship with CollectionFilter for dynamic collections
    public CollectionFilter? Filter { get; private set; }

    private Collection()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    /// <summary>
    /// Creates a new Collection instance
    /// </summary>
    public static Collection Create(
        string name,
        CollectionType type,
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isActive = true,
        bool isFeatured = false,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
        }

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));
        }

        var collection = new Collection
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            ImageUrl = imageUrl,
            Type = type,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            IsFeatured = isFeatured,
            StartDate = startDate,
            EndDate = endDate
        };

        // Auto-populate ExternalId for Integration API compatibility
        collection.SetExternalIdentifiers(externalCode: null, externalId: collection.Id.ToString());

        return collection;
    }

    /// <summary>
    /// Creates a collection from an external system.
    /// </summary>
    public static Collection CreateFromExternal(
        string externalId,
        string name,
        CollectionType type,
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isActive = true,
        bool isFeatured = false,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
        }

        var collection = new Collection
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            ImageUrl = imageUrl,
            Type = type,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            IsFeatured = isFeatured,
            StartDate = startDate,
            EndDate = endDate
        };

        InitializeFromExternal(collection, externalId, externalCode);

        return collection;
    }

    /// <summary>
    /// Updates collection metadata (type is immutable)
    /// </summary>
    public void Update(
        string name,
        string? description,
        string? imageUrl,
        int displayOrder,
        bool isFeatured,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
        }

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));
        }

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Updates collection from external system sync
    /// </summary>
    public void UpdateFromExternal(
        string name,
        string? description,
        string? imageUrl,
        int displayOrder,
        bool isActive,
        bool isFeatured,
        DateTime? startDate,
        DateTime? endDate,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
        }

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        IsFeatured = isFeatured;
        StartDate = startDate;
        EndDate = endDate;

        if (externalCode is not null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    /// <summary>
    /// Sets the collection slug
    /// </summary>
    public void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));
        }

        Slug = slug.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Sets the schedule for the collection
    /// </summary>
    public void SetSchedule(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
    }

    /// <summary>
    /// Adds a product to the collection (manual collections only)
    /// </summary>
    public ProductCollection AddProduct(Guid productId, int displayOrder = 0, bool isFeatured = false)
    {
        if (Type != CollectionType.Manual)
        {
            throw new InvalidOperationException("Cannot add products to a dynamic collection. Use filters instead.");
        }

        if (_productCollections.Count >= MaxProductsPerManualCollection)
        {
            throw new InvalidOperationException($"Collection cannot have more than {MaxProductsPerManualCollection} products.");
        }

        if (_productCollections.Any(pc => pc.ProductId == productId && !pc.IsDeleted))
        {
            throw new InvalidOperationException("Product is already in this collection.");
        }

        var productCollection = ProductCollection.Create(Id, productId, displayOrder, isFeatured);
        _productCollections.Add(productCollection);
        return productCollection;
    }

    /// <summary>
    /// Removes a product from the collection
    /// </summary>
    public void RemoveProduct(Guid productId)
    {
        var productCollection = _productCollections.FirstOrDefault(pc => pc.ProductId == productId && !pc.IsDeleted);
        if (productCollection is not null)
        {
            productCollection.MarkAsDeleted();
        }
    }

    /// <summary>
    /// Clears all products from the collection
    /// </summary>
    public void ClearProducts()
    {
        foreach (var pc in _productCollections.Where(pc => !pc.IsDeleted))
        {
            pc.MarkAsDeleted();
        }
    }

    /// <summary>
    /// Gets all active product IDs in this collection
    /// </summary>
    public IEnumerable<Guid> GetProductIds()
    {
        return _productCollections
            .Where(pc => !pc.IsDeleted)
            .OrderBy(pc => pc.DisplayOrder)
            .Select(pc => pc.ProductId);
    }

    /// <summary>
    /// Sets the filter for dynamic collections
    /// </summary>
    public void SetFilter(CollectionFilter filter)
    {
        if (Type != CollectionType.Dynamic)
        {
            throw new InvalidOperationException("Cannot set filters on a manual collection.");
        }

        Filter = filter;
    }

    /// <summary>
    /// Generates a URL-friendly slug from a name
    /// </summary>
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var slug = name.ToLowerInvariant();

        // Replace Turkish characters
        slug = slug
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Replace spaces and special characters with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from ends
        slug = slug.Trim('-');

        return slug;
    }
}
