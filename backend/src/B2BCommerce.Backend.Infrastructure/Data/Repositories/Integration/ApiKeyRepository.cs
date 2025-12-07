using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories.Integration;

/// <summary>
/// Repository implementation for API keys
/// </summary>
public class ApiKeyRepository : GenericRepository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.ApiClient)
            .Include(x => x.Permissions)
            .Include(x => x.IpWhitelist)
            .FirstOrDefaultAsync(x => x.KeyHash == keyHash, cancellationToken);
    }

    public async Task<ApiKey?> GetByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.KeyPrefix == keyPrefix, cancellationToken);
    }

    public async Task<ApiKey?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.ApiClient)
            .Include(x => x.Permissions)
            .Include(x => x.IpWhitelist)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<ApiKey>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.ApiClientId == clientId)
            .Include(x => x.Permissions)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ApiKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.IsActive &&
                       (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow) &&
                       x.RevokedAt == null)
            .Include(x => x.ApiClient)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ApiKey>> GetExpiringKeysAsync(int daysUntilExpiration, CancellationToken cancellationToken = default)
    {
        var expirationThreshold = DateTime.UtcNow.AddDays(daysUntilExpiration);

        return await _dbSet
            .Where(x => x.IsActive &&
                       x.ExpiresAt != null &&
                       x.ExpiresAt <= expirationThreshold &&
                       x.ExpiresAt > DateTime.UtcNow &&
                       x.RevokedAt == null)
            .Include(x => x.ApiClient)
            .OrderBy(x => x.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateLastUsedAsync(Guid keyId, string ipAddress, CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"ApiKeys\" SET \"LastUsedAt\" = {DateTime.UtcNow}, \"LastUsedIp\" = {ipAddress} WHERE \"Id\" = {keyId}",
            cancellationToken);
    }
}
