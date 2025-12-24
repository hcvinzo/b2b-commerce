using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Collection repository implementation for collection-specific operations
/// </summary>
public class CollectionRepository : GenericRepository<Collection>, ICollectionRepository
{
    public CollectionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a collection by ID with product count
    /// </summary>
    public override async Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filter)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a collection by its slug
    /// </summary>
    public async Task<Collection?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filter)
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a collection by its external ID (for ERP integration)
    /// </summary>
    public async Task<Collection?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a collection with its products (for manual collections)
    /// </summary>
    public async Task<Collection?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.ProductCollections.OrderBy(pc => pc.DisplayOrder))
                .ThenInclude(pc => pc.Product)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a collection with its filter (for dynamic collections)
    /// </summary>
    public async Task<Collection?> GetWithFilterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filter)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a collection with all related data
    /// </summary>
    public async Task<Collection?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filter)
            .Include(c => c.ProductCollections.OrderBy(pc => pc.DisplayOrder))
                .ThenInclude(pc => pc.Product)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all active collections that are currently valid (within date range)
    /// </summary>
    public async Task<IEnumerable<Collection>> GetActiveCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Include(c => c.Filter)
            .Where(c => !c.IsDeleted && c.IsActive)
            .Where(c => (c.StartDate == null || c.StartDate <= now) &&
                       (c.EndDate == null || c.EndDate >= now))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all collections by type
    /// </summary>
    public async Task<IEnumerable<Collection>> GetByTypeAsync(
        CollectionType type,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.Filter)
            .Where(c => c.Type == type && !c.IsDeleted);

        if (activeOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c => c.IsActive &&
                (c.StartDate == null || c.StartDate <= now) &&
                (c.EndDate == null || c.EndDate >= now));
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets featured collections
    /// </summary>
    public async Task<IEnumerable<Collection>> GetFeaturedAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.Filter)
            .Where(c => c.IsFeatured && !c.IsDeleted);

        if (activeOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c => c.IsActive &&
                (c.StartDate == null || c.StartDate <= now) &&
                (c.EndDate == null || c.EndDate >= now));
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a slug is already in use
    /// </summary>
    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.Slug == slug && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a collection exists by external ID
    /// </summary>
    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets the count of products in a manual collection
    /// </summary>
    public async Task<int> GetProductCountAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProductCollection>()
            .CountAsync(pc => pc.CollectionId == collectionId && !pc.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets paginated collections with filtering and sorting
    /// </summary>
    public async Task<(IEnumerable<Collection> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        CollectionType? type = null,
        bool? isActive = null,
        bool? isFeatured = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(c => c.ProductCollections.Where(pc => !pc.IsDeleted))
            .Where(c => !c.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.Slug.ToLower().Contains(searchLower) ||
                (c.Description != null && c.Description.ToLower().Contains(searchLower)));
        }

        // Apply type filter
        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        // Apply active filter
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Apply featured filter
        if (isFeatured.HasValue)
        {
            query = query.Where(c => c.IsFeatured == isFeatured.Value);
        }

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDirection);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Gets paginated products in a manual collection
    /// </summary>
    public async Task<(IEnumerable<(ProductCollection ProductCollection, Product Product)> Items, int TotalCount)> GetManualCollectionProductsPagedAsync(
        Guid collectionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<ProductCollection>()
            .AsNoTracking()
            .Where(pc => pc.CollectionId == collectionId && !pc.IsDeleted)
            .Include(pc => pc.Product)
            .OrderBy(pc => pc.DisplayOrder);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(pc => new { pc, pc.Product })
            .ToListAsync(cancellationToken);

        return (items.Select(x => (x.pc, x.Product)), totalCount);
    }

    /// <summary>
    /// Gets paginated products matching dynamic collection filter criteria
    /// </summary>
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetDynamicCollectionProductsPagedAsync(
        CollectionFilter? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Note: IsActive is a computed property, so we use Status == Active directly
        // Global query filter already excludes IsDeleted
        var query = _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active);

        // Apply filter criteria if filter exists
        if (filter is not null)
        {
            // Materialize filter values to local variables for EF Core translation
            var categoryIds = filter.CategoryIds?.ToList() ?? new List<Guid>();
            var brandIds = filter.BrandIds?.ToList() ?? new List<Guid>();
            var productTypeIds = filter.ProductTypeIds?.ToList() ?? new List<Guid>();

            // Filter by categories
            if (categoryIds.Count > 0)
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId) && !pc.IsDeleted));
            }

            // Filter by brands
            if (brandIds.Count > 0)
            {
                query = query.Where(p => p.BrandId.HasValue && brandIds.Contains(p.BrandId.Value));
            }

            // Filter by product types
            if (productTypeIds.Count > 0)
            {
                query = query.Where(p => p.ProductTypeId.HasValue && productTypeIds.Contains(p.ProductTypeId.Value));
            }

            // Filter by price range
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.ListPrice.Amount >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.ListPrice.Amount <= filter.MaxPrice.Value);
            }
        }

        // Order by name for dynamic collections
        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Replaces all products in a manual collection with new products
    /// </summary>
    public async Task ReplaceProductsAsync(
        Guid collectionId,
        List<(Guid ProductId, int DisplayOrder, bool IsFeatured)> products,
        CancellationToken cancellationToken = default)
    {
        // Soft delete existing products
        var existingProducts = await _context.Set<ProductCollection>()
            .Where(pc => pc.CollectionId == collectionId && !pc.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var pc in existingProducts)
        {
            pc.MarkAsDeleted();
        }

        // Add new products
        foreach (var (productId, displayOrder, isFeatured) in products)
        {
            var productCollection = ProductCollection.Create(collectionId, productId, displayOrder, isFeatured);
            await _context.Set<ProductCollection>().AddAsync(productCollection, cancellationToken);
        }
    }

    /// <summary>
    /// Sets or updates the filter for a dynamic collection
    /// </summary>
    public async Task<CollectionFilter> SetFilterAsync(
        Guid collectionId,
        List<Guid>? categoryIds,
        List<Guid>? brandIds,
        List<Guid>? productTypeIds,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken = default)
    {
        // Check if filter already exists
        var existingFilter = await _context.Set<CollectionFilter>()
            .FirstOrDefaultAsync(cf => cf.CollectionId == collectionId && !cf.IsDeleted, cancellationToken);

        if (existingFilter is not null)
        {
            // Update existing filter
            existingFilter.Update(categoryIds, brandIds, productTypeIds, minPrice, maxPrice);
            return existingFilter;
        }
        else
        {
            // Create new filter
            var filter = CollectionFilter.Create(collectionId, categoryIds, brandIds, productTypeIds, minPrice, maxPrice);
            await _context.Set<CollectionFilter>().AddAsync(filter, cancellationToken);
            return filter;
        }
    }

    private static IQueryable<Collection> ApplySorting(
        IQueryable<Collection> query,
        string? sortBy,
        string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "type" => isDescending
                ? query.OrderByDescending(c => c.Type)
                : query.OrderBy(c => c.Type),
            "displayorder" => isDescending
                ? query.OrderByDescending(c => c.DisplayOrder)
                : query.OrderBy(c => c.DisplayOrder),
            "createdat" => isDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            "startdate" => isDescending
                ? query.OrderByDescending(c => c.StartDate)
                : query.OrderBy(c => c.StartDate),
            _ => query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
        };
    }
}
