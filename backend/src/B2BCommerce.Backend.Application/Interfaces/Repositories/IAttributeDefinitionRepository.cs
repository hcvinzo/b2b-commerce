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
}
