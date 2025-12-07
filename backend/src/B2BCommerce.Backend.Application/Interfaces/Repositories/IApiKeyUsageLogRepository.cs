using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for API key usage log operations
/// </summary>
public interface IApiKeyUsageLogRepository
{
    /// <summary>
    /// Adds a new usage log entry
    /// </summary>
    Task AddAsync(ApiKeyUsageLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple usage log entries
    /// </summary>
    Task AddBatchAsync(IEnumerable<ApiKeyUsageLog> logs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated usage logs based on filter criteria
    /// </summary>
    Task<PagedResult<ApiKeyUsageLog>> GetLogsAsync(UsageLogFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for a key within a date range
    /// </summary>
    Task<ApiKeyUsageStatsDto> GetStatsAsync(Guid apiKeyId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the request count for a key since a specific time
    /// </summary>
    Task<long> GetRequestCountAsync(Guid apiKeyId, DateTime since, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old log entries
    /// </summary>
    Task DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
