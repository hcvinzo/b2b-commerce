using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

public class GeoLocationRepository : GenericRepository<GeoLocation>, IGeoLocationRepository
{
    public GeoLocationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<GeoLocation?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ExternalId == externalId, cancellationToken);
    }

    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(l => l.ExternalId == externalId, cancellationToken);
    }

    public async Task<GeoLocation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<GeoLocation>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.ParentId == parentId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GeoLocation>> GetByTypeIdAsync(Guid typeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.GeoLocationTypeId == typeId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GeoLocation>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.ParentId == parentId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GeoLocation>> GetRootLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.ParentId == null)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }
}
