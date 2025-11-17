using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Customer repository interface for customer-specific operations
/// </summary>
public interface ICustomerRepository : IGenericRepository<Customer>
{
    /// <summary>
    /// Gets a customer by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer if found, null otherwise</returns>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by tax number
    /// </summary>
    /// <param name="taxNumber">Tax number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer if found, null otherwise</returns>
    Task<Customer?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all customers that are not yet approved
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unapproved customers</returns>
    Task<IEnumerable<Customer>> GetUnapprovedCustomersAsync(CancellationToken cancellationToken = default);
}
