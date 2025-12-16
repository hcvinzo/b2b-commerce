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

    /// <summary>
    /// Gets a product by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<Product?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product with all related data by external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product with related data</returns>
    Task<Product?> GetWithDetailsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product with all related data by SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product with related data</returns>
    Task<Product?> GetWithDetailsBySKUAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product with all related data by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product with related data</returns>
    Task<Product?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of variants for a main product
    /// </summary>
    /// <param name="mainProductId">Main product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of variants</returns>
    Task<int> GetVariantCountAsync(Guid mainProductId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all variants of a main product
    /// </summary>
    /// <param name="mainProductId">Main product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of variant products</returns>
    Task<IEnumerable<Product>> GetVariantsAsync(Guid mainProductId, CancellationToken cancellationToken = default);
}
