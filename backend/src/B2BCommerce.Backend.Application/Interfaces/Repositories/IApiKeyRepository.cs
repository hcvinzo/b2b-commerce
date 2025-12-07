using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for API key operations
/// </summary>
public interface IApiKeyRepository : IGenericRepository<ApiKey>
{
    /// <summary>
    /// Gets a key by its hash
    /// </summary>
    Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a key by its prefix
    /// </summary>
    Task<ApiKey?> GetByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a key with all details (client, permissions, IP whitelist)
    /// </summary>
    Task<ApiKey?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys for a client
    /// </summary>
    Task<List<ApiKey>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (non-expired, non-revoked) keys
    /// </summary>
    Task<List<ApiKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets keys that will expire within the specified days
    /// </summary>
    Task<List<ApiKey>> GetExpiringKeysAsync(int daysUntilExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last used timestamp and IP for a key
    /// </summary>
    Task UpdateLastUsedAsync(Guid keyId, string ipAddress, CancellationToken cancellationToken = default);
}
