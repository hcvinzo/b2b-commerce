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
}
