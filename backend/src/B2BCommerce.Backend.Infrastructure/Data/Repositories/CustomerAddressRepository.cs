using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Customer address repository implementation
/// </summary>
public class CustomerAddressRepository : GenericRepository<CustomerAddress>, ICustomerAddressRepository
{
    public CustomerAddressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.GeoLocation)
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerAddress?> GetDefaultAddressAsync(Guid customerId, CustomerAddressType addressType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.GeoLocation)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.AddressType == addressType && a.IsDefault, cancellationToken);
    }

    public async Task<IEnumerable<CustomerAddress>> GetByAddressTypeAsync(Guid customerId, CustomerAddressType addressType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.GeoLocation)
            .Where(a => a.CustomerId == customerId && a.AddressType == addressType)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Title)
            .ToListAsync(cancellationToken);
    }
}
