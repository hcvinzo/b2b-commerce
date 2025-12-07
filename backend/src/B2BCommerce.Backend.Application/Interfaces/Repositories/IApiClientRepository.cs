using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for API client operations
/// </summary>
public interface IApiClientRepository : IGenericRepository<ApiClient>
{
    /// <summary>
    /// Gets a client by name
    /// </summary>
    Task<ApiClient?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a client with all associated keys loaded
    /// </summary>
    Task<ApiClient?> GetWithKeysAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a client name is unique
    /// </summary>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active clients
    /// </summary>
    Task<List<ApiClient>> GetActiveClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active keys for a client
    /// </summary>
    Task<int> GetActiveKeyCountAsync(Guid clientId, CancellationToken cancellationToken = default);
}
