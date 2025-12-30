using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
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
    /// Override GetByIdAsync to include Contacts and Addresses
    /// </summary>
    public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .Include(c => c.Addresses)
                .ThenInclude(a => a.GeoLocation)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to include Contacts for list display
    /// </summary>
    public override async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Contacts)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ExternalId == externalId, cancellationToken);
    }

    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.ExternalId == externalId, cancellationToken);
    }

    public async Task<Customer?> GetByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TaxNo == taxNo, cancellationToken);
    }

    public async Task<bool> ExistsByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.TaxNo == taxNo, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Status == status)
            .OrderBy(c => c.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetPendingCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Status == CustomerStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
