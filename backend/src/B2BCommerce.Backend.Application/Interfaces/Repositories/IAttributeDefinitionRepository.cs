using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for attribute definition operations
/// </summary>
public interface IAttributeDefinitionRepository : IGenericRepository<AttributeDefinition>
{
    /// <summary>
    /// Gets an attribute definition by its code
    /// </summary>
    /// <param name="code">Attribute code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AttributeDefinition if found, null otherwise</returns>
    Task<AttributeDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all filterable attribute definitions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of filterable attribute definitions</returns>
    Task<IEnumerable<AttributeDefinition>> GetFilterableAttributesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attribute definition with predefined values loaded
    /// </summary>
    /// <param name="id">Attribute definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AttributeDefinition with predefined values</returns>
    Task<AttributeDefinition?> GetWithPredefinedValuesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an attribute definition by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AttributeDefinition if found, null otherwise</returns>
    Task<AttributeDefinition?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attribute definition with predefined values by external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AttributeDefinition with predefined values</returns>
    Task<AttributeDefinition?> GetWithPredefinedValuesByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an attribute definition exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a predefined value to the context for tracking
    /// </summary>
    /// <param name="value">The AttributeValue to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAttributeValueAsync(AttributeValue value, CancellationToken cancellationToken = default);
}
