using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Category repository interface for category-specific operations
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>
    /// Gets all subcategories of a parent category
    /// </summary>
    /// <param name="parentCategoryId">Parent category identifier (null for root categories)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of subcategories</returns>
    Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
}
