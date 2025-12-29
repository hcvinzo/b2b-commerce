using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

public class GeoLocationTypeRepository : GenericRepository<GeoLocationType>, IGeoLocationTypeRepository
{
    public GeoLocationTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<GeoLocationType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.Name == name, cancellationToken);
    }
}
