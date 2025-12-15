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

    /// <summary>
    /// Gets a category by its external ID (primary key for integration)
    /// </summary>
    /// <param name="externalId">External system ID (upsert key)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<Category?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its external code (optional reference)
    /// </summary>
    /// <param name="externalCode">External system code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<Category?> GetByExternalCodeAsync(string externalCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category exists by its external code
    /// </summary>
    /// <param name="externalCode">External system code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalCodeAsync(string externalCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its URL-friendly slug
    /// </summary>
    /// <param name="slug">URL-friendly slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
