using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for product type operations
/// </summary>
public interface IProductTypeRepository : IGenericRepository<ProductType>
{
    /// <summary>
    /// Gets a product type by its code
    /// </summary>
    /// <param name="code">Product type code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductType if found, null otherwise</returns>
    Task<ProductType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active product types
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active product types</returns>
    Task<IEnumerable<ProductType>> GetActiveProductTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product type with attributes loaded
    /// </summary>
    /// <param name="id">Product type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductType with attributes</returns>
    Task<ProductType?> GetWithAttributesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product type by code with attributes loaded
    /// </summary>
    /// <param name="code">Product type code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductType with attributes</returns>
    Task<ProductType?> GetByCodeWithAttributesAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product types with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of items and total count</returns>
    Task<(IEnumerable<ProductType> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product type by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductType if found, null otherwise</returns>
    Task<ProductType?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product type with attributes by external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductType with attributes</returns>
    Task<ProductType?> GetWithAttributesByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product type exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a product type attribute to the context for tracking
    /// </summary>
    /// <param name="attribute">The ProductTypeAttribute to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddProductTypeAttributeAsync(ProductTypeAttribute attribute, CancellationToken cancellationToken = default);
}
