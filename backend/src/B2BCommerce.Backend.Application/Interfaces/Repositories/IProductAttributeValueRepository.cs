using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for product attribute value operations
/// </summary>
public interface IProductAttributeValueRepository : IGenericRepository<ProductAttributeValue>
{
    /// <summary>
    /// Gets all attribute values for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of product attribute values</returns>
    Task<IEnumerable<ProductAttributeValue>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attribute value for a specific product and attribute definition
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="attributeDefinitionId">Attribute definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductAttributeValue if found, null otherwise</returns>
    Task<ProductAttributeValue?> GetByProductAndAttributeAsync(Guid productId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products with a specific attribute value
    /// </summary>
    /// <param name="attributeDefinitionId">Attribute definition ID</param>
    /// <param name="attributeValueId">Attribute value ID (for Select types)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of product IDs</returns>
    Task<IEnumerable<Guid>> GetProductIdsByAttributeValueAsync(Guid attributeDefinitionId, Guid attributeValueId, CancellationToken cancellationToken = default);
}
