using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories.Integration;

/// <summary>
/// Repository implementation for API clients
/// </summary>
public class ApiClientRepository : GenericRepository<ApiClient>, IApiClientRepository
{
    public ApiClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApiClient?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<ApiClient?> GetWithKeysAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.ApiKeys)
                .ThenInclude(k => k.Permissions)
            .Include(x => x.ApiKeys)
                .ThenInclude(k => k.IpWhitelist)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(x => x.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<List<ApiClient>> GetActiveClientsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveKeyCountAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ApiKey>()
            .Where(k => k.ApiClientId == clientId &&
                       k.IsActive &&
                       (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow) &&
                       k.RevokedAt == null)
            .CountAsync(cancellationToken);
    }
}
