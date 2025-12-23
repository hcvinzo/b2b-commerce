using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

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
    /// Gets attributes for a customer by type
    /// </summary>
    Task<IEnumerable<CustomerAttribute>> GetByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerAttributeType attributeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all attributes for a customer by type
    /// </summary>
    Task DeleteByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerAttributeType attributeType,
        CancellationToken cancellationToken = default);
}
