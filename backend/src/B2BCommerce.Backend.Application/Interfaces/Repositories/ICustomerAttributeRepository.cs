using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for CustomerAttribute entity
/// </summary>
public interface ICustomerAttributeRepository : IGenericRepository<CustomerAttribute>
{
    /// <summary>
    /// Gets all attributes for a customer
    /// </summary>
    Task<IEnumerable<CustomerAttribute>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attributes for a customer by attribute definition ID
    /// </summary>
    Task<CustomerAttribute?> GetByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attributes for a customer by attribute definition code
    /// </summary>
    Task<CustomerAttribute?> GetByCustomerIdAndDefinitionCodeAsync(
        Guid customerId,
        string definitionCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all attributes for a customer by attribute definition ID
    /// </summary>
    Task DeleteByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default);
}
