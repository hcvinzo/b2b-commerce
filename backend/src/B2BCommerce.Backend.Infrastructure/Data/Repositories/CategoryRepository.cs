using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Category repository implementation for category-specific operations
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all subcategories of a parent category
    /// </summary>
    /// <param name="parentCategoryId">Parent category identifier (null for root categories)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of subcategories</returns>
    public async Task<IEnumerable<Category>> GetByParentIdAsync(
        Guid? parentCategoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all active categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a category by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    public async Task<Category?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a category exists by its external ID
    /// </summary>
    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a category by its external code (optional reference for LOGO ERP)
    /// </summary>
    public async Task<Category?> GetByExternalCodeAsync(string externalCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.ExternalCode == externalCode && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a category exists by its external code
    /// </summary>
    public async Task<bool> ExistsByExternalCodeAsync(string externalCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.ExternalCode == externalCode && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a category by its URL-friendly slug
    /// </summary>
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, cancellationToken);
    }
}
