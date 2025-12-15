using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for product category operations
/// </summary>
public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
{
    /// <summary>
    /// Gets all category assignments for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of product categories</returns>
    Task<IEnumerable<ProductCategory>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all product assignments for a category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of product categories</returns>
    Task<IEnumerable<ProductCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary category for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Primary ProductCategory if found, null otherwise</returns>
    Task<ProductCategory?> GetPrimaryCategoryAsync(Guid productId, CancellationToken cancellationToken = default);
}
