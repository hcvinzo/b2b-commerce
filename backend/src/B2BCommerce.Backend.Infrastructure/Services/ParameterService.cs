using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Parameters;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Service for managing system configuration parameters
/// </summary>
public class ParameterService : IParameterService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ParameterService> _logger;
    private const string CacheKeyPrefix = "param_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ParameterService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<ParameterService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<Result<PagedResult<ParameterListDto>>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        ParameterType? parameterType = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SystemConfigurations.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Key.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower) ||
                p.Value.ToLower().Contains(searchLower));
        }

        if (parameterType.HasValue)
        {
            query = query.Where(p => p.ParameterType == parameterType.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Key)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ParameterListDto
            {
                Id = p.Id,
                Key = p.Key,
                Value = p.Value,
                Category = p.Category,
                Description = p.Description,
                IsEditable = p.IsEditable,
                ParameterType = p.ParameterType,
                ValueType = p.ValueType,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ParameterListDto>(items, totalCount, page, pageSize);
        return Result<PagedResult<ParameterListDto>>.Success(result);
    }

    public async Task<Result<ParameterDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var parameter = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (parameter is null)
        {
            return Result<ParameterDetailDto>.Failure("Parameter not found", "PARAMETER_NOT_FOUND");
        }

        return Result<ParameterDetailDto>.Success(MapToDetailDto(parameter));
    }

    public async Task<Result<ParameterDetailDto>> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var parameter = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

        if (parameter is null)
        {
            return Result<ParameterDetailDto>.Failure($"Parameter '{key}' not found", "PARAMETER_NOT_FOUND");
        }

        return Result<ParameterDetailDto>.Success(MapToDetailDto(parameter));
    }

    public async Task<Result<ParameterDetailDto>> CreateAsync(
        CreateParameterDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check if key already exists
        var exists = await _context.SystemConfigurations
            .AnyAsync(p => p.Key == dto.Key, cancellationToken);

        if (exists)
        {
            return Result<ParameterDetailDto>.Failure(
                $"Parameter with key '{dto.Key}' already exists",
                "PARAMETER_KEY_EXISTS");
        }

        try
        {
            var parameter = SystemConfiguration.Create(
                dto.Key,
                dto.Value,
                dto.Description,
                dto.ParameterType,
                dto.ValueType,
                dto.IsEditable);

            parameter.CreatedBy = createdBy;

            _context.SystemConfigurations.Add(parameter);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parameter created: {Key} by {CreatedBy}", dto.Key, createdBy);

            // Invalidate cache
            InvalidateCache(dto.Key);

            return Result<ParameterDetailDto>.Success(MapToDetailDto(parameter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create parameter: {Key}", dto.Key);
            return Result<ParameterDetailDto>.Failure(ex.Message, "PARAMETER_CREATE_FAILED");
        }
    }

    public async Task<Result<ParameterDetailDto>> UpdateAsync(
        Guid id,
        UpdateParameterDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var parameter = await _context.SystemConfigurations
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (parameter is null)
        {
            return Result<ParameterDetailDto>.Failure("Parameter not found", "PARAMETER_NOT_FOUND");
        }

        try
        {
            // Update value if provided
            if (dto.Value is not null)
            {
                parameter.UpdateValue(dto.Value);
            }

            // Update metadata
            parameter.Update(dto.Description, dto.IsEditable);

            parameter.UpdatedBy = updatedBy;
            parameter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parameter updated: {Key} by {UpdatedBy}", parameter.Key, updatedBy);

            // Invalidate cache
            InvalidateCache(parameter.Key);

            return Result<ParameterDetailDto>.Success(MapToDetailDto(parameter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update parameter: {Id}", id);
            return Result<ParameterDetailDto>.Failure(ex.Message, "PARAMETER_UPDATE_FAILED");
        }
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var parameter = await _context.SystemConfigurations
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (parameter is null)
        {
            return Result.Failure("Parameter not found", "PARAMETER_NOT_FOUND");
        }

        var key = parameter.Key;
        parameter.MarkAsDeleted(deletedBy);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Parameter deleted: {Key} by {DeletedBy}", key, deletedBy);

        // Invalidate cache
        InvalidateCache(key);

        return Result.Success();
    }

    public async Task<Result<List<string>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _context.SystemConfigurations
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        return Result<List<string>>.Success(categories);
    }

    #endregion

    #region Typed Value Getters

    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";

        if (_cache.TryGetValue(cacheKey, out SystemConfiguration? cached) && cached is not null)
        {
            return cached.GetTypedValue<T>();
        }

        var parameter = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

        if (parameter is null)
        {
            return default;
        }

        _cache.Set(cacheKey, parameter, CacheDuration);
        return parameter.GetTypedValue<T>();
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetValueAsync<string>(key, cancellationToken);
    }

    public async Task<int?> GetIntAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetValueAsync<int?>(key, cancellationToken);
    }

    public async Task<bool?> GetBoolAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetValueAsync<bool?>(key, cancellationToken);
    }

    public async Task<decimal?> GetDecimalAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetValueAsync<decimal?>(key, cancellationToken);
    }

    #endregion

    #region Private Methods

    private static ParameterDetailDto MapToDetailDto(SystemConfiguration parameter)
    {
        return new ParameterDetailDto
        {
            Id = parameter.Id,
            Key = parameter.Key,
            Value = parameter.Value,
            Category = parameter.Category,
            Description = parameter.Description,
            IsEditable = parameter.IsEditable,
            ParameterType = parameter.ParameterType,
            ValueType = parameter.ValueType,
            CreatedAt = parameter.CreatedAt,
            CreatedBy = parameter.CreatedBy,
            UpdatedAt = parameter.UpdatedAt,
            UpdatedBy = parameter.UpdatedBy
        };
    }

    private void InvalidateCache(string key)
    {
        _cache.Remove($"{CacheKeyPrefix}{key}");
    }

    #endregion
}
