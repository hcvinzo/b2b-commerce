using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for API key management operations
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Gets a key by ID with details
    /// </summary>
    Task<Result<ApiKeyDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys for a client
    /// </summary>
    Task<Result<List<ApiKeyListDto>>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new API key
    /// </summary>
    Task<Result<CreateApiKeyResponseDto>> CreateAsync(CreateApiKeyDto dto, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing key (name, rate limit, expiration)
    /// </summary>
    Task<Result<ApiKeyDetailDto>> UpdateAsync(Guid id, UpdateApiKeyDto dto, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a key
    /// </summary>
    Task<Result> RevokeAsync(Guid id, RevokeApiKeyDto dto, string revokedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a key (creates new, revokes old)
    /// </summary>
    Task<Result<CreateApiKeyResponseDto>> RotateAsync(Guid id, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates key permissions
    /// </summary>
    Task<Result> UpdatePermissionsAsync(Guid keyId, UpdateApiKeyPermissionsDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an IP to the whitelist
    /// </summary>
    Task<Result> AddIpToWhitelistAsync(Guid keyId, AddIpWhitelistDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an IP from the whitelist
    /// </summary>
    Task<Result> RemoveIpFromWhitelistAsync(Guid keyId, Guid whitelistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an API key for authentication
    /// </summary>
    Task<ApiKeyValidationResult> ValidateKeyAsync(string plainTextKey, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage logs for a key
    /// </summary>
    Task<Result<PagedResult<ApiKeyUsageLogDto>>> GetUsageLogsAsync(UsageLogFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for a key
    /// </summary>
    Task<Result<ApiKeyUsageStatsDto>> GetUsageStatsAsync(Guid keyId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
