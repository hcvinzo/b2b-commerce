using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Collection repository interface for collection-specific operations
/// </summary>
public interface ICollectionRepository : IGenericRepository<Collection>
{
    /// <summary>
    /// Gets a collection by its slug
    /// </summary>
    /// <param name="slug">Collection slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection if found, null otherwise</returns>
    Task<Collection?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a collection by its external ID (for ERP integration)
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection if found, null otherwise</returns>
    Task<Collection?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a collection with its products (for manual collections)
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection with ProductCollections loaded</returns>
    Task<Collection?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a collection with its filter (for dynamic collections)
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection with Filter loaded</returns>
    Task<Collection?> GetWithFilterAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a collection with all related data
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection with all navigation properties loaded</returns>
    Task<Collection?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active collections that are currently valid (within date range)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of currently active collections</returns>
    Task<IEnumerable<Collection>> GetActiveCollectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all collections by type
    /// </summary>
    /// <param name="type">Collection type (Manual or Dynamic)</param>
    /// <param name="activeOnly">Whether to return only active collections</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of collections</returns>
    Task<IEnumerable<Collection>> GetByTypeAsync(CollectionType type, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets featured collections
    /// </summary>
    /// <param name="activeOnly">Whether to return only currently active collections</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of featured collections</returns>
    Task<IEnumerable<Collection>> GetFeaturedAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slug is already in use
    /// </summary>
    /// <param name="slug">Slug to check</param>
    /// <param name="excludeId">Optional collection ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if slug exists, false otherwise</returns>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a collection exists by external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of products in a manual collection
    /// </summary>
    /// <param name="collectionId">Collection ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of products</returns>
    Task<int> GetProductCountAsync(Guid collectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated collections with filtering and sorting
    /// </summary>
    Task<(IEnumerable<Collection> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        CollectionType? type = null,
        bool? isActive = null,
        bool? isFeatured = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated products in a manual collection
    /// </summary>
    Task<(IEnumerable<(ProductCollection ProductCollection, Product Product)> Items, int TotalCount)> GetManualCollectionProductsPagedAsync(
        Guid collectionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated products matching dynamic collection filter criteria
    /// </summary>
    Task<(IEnumerable<Product> Items, int TotalCount)> GetDynamicCollectionProductsPagedAsync(
        CollectionFilter? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces all products in a manual collection with new products.
    /// Soft deletes existing products and adds new ones.
    /// </summary>
    /// <param name="collectionId">Collection ID</param>
    /// <param name="products">List of (ProductId, DisplayOrder, IsFeatured) tuples</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ReplaceProductsAsync(
        Guid collectionId,
        List<(Guid ProductId, int DisplayOrder, bool IsFeatured)> products,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or updates the filter for a dynamic collection.
    /// </summary>
    /// <param name="collectionId">Collection ID</param>
    /// <param name="categoryIds">Category IDs to filter by</param>
    /// <param name="brandIds">Brand IDs to filter by</param>
    /// <param name="productTypeIds">Product Type IDs to filter by</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created or updated filter</returns>
    Task<CollectionFilter> SetFilterAsync(
        Guid collectionId,
        List<Guid>? categoryIds,
        List<Guid>? brandIds,
        List<Guid>? productTypeIds,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken = default);
}
