using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Product repository interface for product-specific operations
/// </summary>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products in a specific category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="includeInactive">Whether to include inactive products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products</returns>
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name, SKU, or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="brandId">Optional brand filter</param>
    /// <param name="activeOnly">Whether to return only active products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching products</returns>
    Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        Guid? categoryId = null,
        Guid? brandId = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
}
