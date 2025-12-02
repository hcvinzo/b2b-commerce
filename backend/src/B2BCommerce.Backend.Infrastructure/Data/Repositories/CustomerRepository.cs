using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Customer repository implementation for customer-specific operations
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a customer by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer if found, null otherwise</returns>
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Email.Value == email && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a customer by tax number
    /// </summary>
    /// <param name="taxNumber">Tax number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer if found, null otherwise</returns>
    public async Task<Customer?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.TaxNumber.Value == taxNumber && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all customers that are not yet approved
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unapproved customers</returns>
    public async Task<IEnumerable<Customer>> GetUnapprovedCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.IsApproved && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
