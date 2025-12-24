using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Product repository implementation for product-specific operations
/// </summary>
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all products with necessary includes for ProductService
    /// </summary>
    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a product by ID with all necessary includes for ProductService
    /// </summary>
    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductType)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.AttributeDefinition)
                    .ThenInclude(ad => ad!.PredefinedValues)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product if found, null otherwise</returns>
    public async Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="includeInactive">Whether to include inactive products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products</returns>
    public async Task<IEnumerable<Product>> GetByCategoryAsync(
        Guid categoryId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId) && !p.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches products by name, SKU, or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="brandId">Optional brand filter</param>
    /// <param name="activeOnly">Whether to return only active products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching products</returns>
    public async Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        Guid? categoryId = null,
        Guid? brandId = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lowerSearchTerm) ||
                p.SKU.ToLower().Contains(lowerSearchTerm) ||
                p.Description.ToLower().Contains(lowerSearchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a product by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    public async Task<Product?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ExternalId == externalId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets product with all related data by external ID
    /// </summary>
    public async Task<Product?> GetWithDetailsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductType)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets product with all related data by SKU
    /// </summary>
    public async Task<Product?> GetWithDetailsBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductType)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets product with all related data by ID
    /// </summary>
    public async Task<Product?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductType)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.MainProduct)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a product exists by its external ID
    /// </summary>
    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.ExternalId == externalId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets the count of variants for a main product
    /// </summary>
    public async Task<int> GetVariantCountAsync(Guid mainProductId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(p => p.MainProductId == mainProductId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all variants of a main product
    /// </summary>
    public async Task<IEnumerable<Product>> GetVariantsAsync(Guid mainProductId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.MainProductId == mainProductId && !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a product with all source relations loaded (for relation management)
    /// </summary>
    public async Task<Product?> GetWithRelationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SourceRelations)
                .ThenInclude(r => r.RelatedProduct)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }
}
