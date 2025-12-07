using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories.Integration;

/// <summary>
/// Repository implementation for API key usage logs
/// </summary>
public class ApiKeyUsageLogRepository : IApiKeyUsageLogRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ApiKeyUsageLog> _dbSet;

    public ApiKeyUsageLogRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ApiKeyUsageLog>();
    }

    public async Task AddAsync(ApiKeyUsageLog log, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBatchAsync(IEnumerable<ApiKeyUsageLog> logs, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(logs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<ApiKeyUsageLog>> GetLogsAsync(UsageLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (filter.ApiKeyId.HasValue)
            query = query.Where(x => x.ApiKeyId == filter.ApiKeyId.Value);

        if (filter.ApiClientId.HasValue)
            query = query.Where(x => x.ApiKey.ApiClientId == filter.ApiClientId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.RequestTimestamp >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.RequestTimestamp <= filter.ToDate.Value);

        if (!string.IsNullOrEmpty(filter.Endpoint))
            query = query.Where(x => x.Endpoint.Contains(filter.Endpoint));

        if (filter.StatusCode.HasValue)
            query = query.Where(x => x.ResponseStatusCode == filter.StatusCode.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.RequestTimestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ApiKeyUsageLog>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<ApiKeyUsageStatsDto> GetStatsAsync(Guid apiKeyId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var logs = await _dbSet
            .Where(x => x.ApiKeyId == apiKeyId &&
                   x.RequestTimestamp >= fromDate &&
                   x.RequestTimestamp <= toDate)
            .ToListAsync(cancellationToken);

        var apiKey = await _context.Set<ApiKey>()
            .FirstOrDefaultAsync(x => x.Id == apiKeyId, cancellationToken);

        return new ApiKeyUsageStatsDto
        {
            ApiKeyId = apiKeyId,
            KeyPrefix = apiKey?.KeyPrefix ?? "",
            KeyName = apiKey?.Name ?? "",
            TotalRequests = logs.Count,
            SuccessfulRequests = logs.Count(x => x.ResponseStatusCode >= 200 && x.ResponseStatusCode < 400),
            FailedRequests = logs.Count(x => x.ResponseStatusCode >= 400),
            AverageResponseTimeMs = logs.Any() ? logs.Average(x => x.ResponseTimeMs) : 0,
            FirstUsedAt = logs.MinBy(x => x.RequestTimestamp)?.RequestTimestamp,
            LastUsedAt = logs.MaxBy(x => x.RequestTimestamp)?.RequestTimestamp,
            RequestsByEndpoint = logs.GroupBy(x => x.Endpoint)
                .ToDictionary(g => g.Key, g => (long)g.Count()),
            RequestsByStatusCode = logs.GroupBy(x => x.ResponseStatusCode)
                .ToDictionary(g => g.Key, g => (long)g.Count())
        };
    }

    public async Task<long> GetRequestCountAsync(Guid apiKeyId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.ApiKeyId == apiKeyId && x.RequestTimestamp >= since)
            .LongCountAsync(cancellationToken);
    }

    public async Task DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM \"ApiKeyUsageLogs\" WHERE \"RequestTimestamp\" < {olderThan}",
            cancellationToken);
    }
}
