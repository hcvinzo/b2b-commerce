using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities.Integration;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Infrastructure.Services.Integration;

/// <summary>
/// Service implementation for API key management
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiKeyGenerator _keyGenerator;
    private readonly IMapper _mapper;

    public ApiKeyService(
        IUnitOfWork unitOfWork,
        IApiKeyGenerator keyGenerator,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _keyGenerator = keyGenerator;
        _mapper = mapper;
    }

    public async Task<Result<ApiKeyDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id, cancellationToken);

        if (key is null)
        {
            return Result<ApiKeyDetailDto>.Failure("API key not found", "KEY_NOT_FOUND");
        }

        var dto = _mapper.Map<ApiKeyDetailDto>(key);
        return Result<ApiKeyDetailDto>.Success(dto);
    }

    public async Task<Result<List<ApiKeyListDto>>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
        {
            return Result<List<ApiKeyListDto>>.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        var keys = await _unitOfWork.ApiKeys.GetByClientIdAsync(clientId, cancellationToken);
        var dtos = _mapper.Map<List<ApiKeyListDto>>(keys);

        return Result<List<ApiKeyListDto>>.Success(dtos);
    }

    public async Task<Result<CreateApiKeyResponseDto>> CreateAsync(
        CreateApiKeyDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Verify client exists and is active
        var client = await _unitOfWork.ApiClients.GetByIdAsync(dto.ApiClientId, cancellationToken);
        if (client is null)
        {
            return Result<CreateApiKeyResponseDto>.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        if (!client.IsActive)
        {
            return Result<CreateApiKeyResponseDto>.Failure("Cannot create key for inactive client", "CLIENT_INACTIVE");
        }

        // Validate permissions
        var invalidScopes = dto.Permissions.Where(p => !IntegrationPermissionScopes.IsValidScope(p)).ToList();
        if (invalidScopes.Any())
        {
            return Result<CreateApiKeyResponseDto>.Failure(
                $"Invalid permission scopes: {string.Join(", ", invalidScopes)}",
                "INVALID_PERMISSIONS");
        }

        // Generate the key
        var (plainTextKey, keyHash, keyPrefix) = _keyGenerator.GenerateKey();

        // Create the API key entity
        var apiKey = new ApiKey(
            dto.ApiClientId,
            keyHash,
            keyPrefix,
            dto.Name,
            dto.RateLimitPerMinute,
            dto.ExpiresAt,
            createdBy);

        // Add permissions
        foreach (var scope in dto.Permissions)
        {
            apiKey.AddPermission(scope);
        }

        // Add IP whitelist if provided - IpWhitelist is List<string> in the DTO
        if (dto.IpWhitelist?.Any() == true)
        {
            foreach (var ip in dto.IpWhitelist)
            {
                apiKey.AddIpToWhitelist(ip);
            }
        }

        await _unitOfWork.ApiKeys.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateApiKeyResponseDto
        {
            Id = apiKey.Id,
            PlainTextKey = plainTextKey,
            KeyPrefix = keyPrefix,
            Name = apiKey.Name,
            ExpiresAt = apiKey.ExpiresAt,
            Permissions = dto.Permissions
        };

        return Result<CreateApiKeyResponseDto>.Success(response);
    }

    public async Task<Result<ApiKeyDetailDto>> UpdateAsync(
        Guid id,
        UpdateApiKeyDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id, cancellationToken);

        if (key is null)
        {
            return Result<ApiKeyDetailDto>.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (key.IsRevoked())
        {
            return Result<ApiKeyDetailDto>.Failure("Cannot update revoked key", "KEY_REVOKED");
        }

        key.UpdateName(dto.Name);
        key.UpdateRateLimit(dto.RateLimitPerMinute);

        if (dto.ExpiresAt != key.ExpiresAt)
        {
            key.UpdateExpiration(dto.ExpiresAt);
        }

        key.UpdatedBy = updatedBy;

        _unitOfWork.ApiKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<ApiKeyDetailDto>(key);
        return Result<ApiKeyDetailDto>.Success(resultDto);
    }

    public async Task<Result> RevokeAsync(
        Guid id,
        RevokeApiKeyDto dto,
        string revokedBy,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetByIdAsync(id, cancellationToken);

        if (key is null)
        {
            return Result.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (key.IsRevoked())
        {
            return Result.Failure("Key is already revoked", "ALREADY_REVOKED");
        }

        key.Revoke(dto.Reason, revokedBy);

        _unitOfWork.ApiKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CreateApiKeyResponseDto>> RotateAsync(
        Guid id,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var oldKey = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id, cancellationToken);

        if (oldKey is null)
        {
            return Result<CreateApiKeyResponseDto>.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (oldKey.IsRevoked())
        {
            return Result<CreateApiKeyResponseDto>.Failure("Cannot rotate revoked key", "KEY_REVOKED");
        }

        // Generate new key
        var (plainTextKey, keyHash, keyPrefix) = _keyGenerator.GenerateKey();

        // Create new key with same settings
        var newKey = new ApiKey(
            oldKey.ApiClientId,
            keyHash,
            keyPrefix,
            $"{oldKey.Name} (rotated)",
            oldKey.RateLimitPerMinute,
            oldKey.ExpiresAt,
            createdBy);

        // Copy permissions
        foreach (var permission in oldKey.Permissions)
        {
            newKey.AddPermission(permission.Scope);
        }

        // Copy IP whitelist
        foreach (var ip in oldKey.IpWhitelist)
        {
            newKey.AddIpToWhitelist(ip.IpAddress, ip.Description);
        }

        // Revoke old key
        oldKey.Revoke("Key rotated", createdBy);

        await _unitOfWork.ApiKeys.AddAsync(newKey, cancellationToken);
        _unitOfWork.ApiKeys.Update(oldKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateApiKeyResponseDto
        {
            Id = newKey.Id,
            PlainTextKey = plainTextKey,
            KeyPrefix = keyPrefix,
            Name = newKey.Name,
            ExpiresAt = newKey.ExpiresAt,
            Permissions = newKey.Permissions.Select(p => p.Scope).ToList()
        };

        return Result<CreateApiKeyResponseDto>.Success(response);
    }

    public async Task<Result> UpdatePermissionsAsync(
        Guid keyId,
        UpdateApiKeyPermissionsDto dto,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId, cancellationToken);

        if (key is null)
        {
            return Result.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (key.IsRevoked())
        {
            return Result.Failure("Cannot update revoked key", "KEY_REVOKED");
        }

        // Validate new permissions
        var invalidScopes = dto.Permissions.Where(p => !IntegrationPermissionScopes.IsValidScope(p)).ToList();
        if (invalidScopes.Any())
        {
            return Result.Failure(
                $"Invalid permission scopes: {string.Join(", ", invalidScopes)}",
                "INVALID_PERMISSIONS");
        }

        // Clear existing and add new permissions
        key.ClearPermissions();
        foreach (var scope in dto.Permissions)
        {
            key.AddPermission(scope);
        }

        _unitOfWork.ApiKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddIpToWhitelistAsync(
        Guid keyId,
        AddIpWhitelistDto dto,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId, cancellationToken);

        if (key is null)
        {
            return Result.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (key.IsRevoked())
        {
            return Result.Failure("Cannot update revoked key", "KEY_REVOKED");
        }

        // Check for duplicate
        if (key.IpWhitelist.Any(ip => ip.IpAddress.Equals(dto.IpAddress, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure("IP address already whitelisted", "DUPLICATE_IP");
        }

        key.AddIpToWhitelist(dto.IpAddress, dto.Description);

        _unitOfWork.ApiKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveIpFromWhitelistAsync(
        Guid keyId,
        Guid whitelistId,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId, cancellationToken);

        if (key is null)
        {
            return Result.Failure("API key not found", "KEY_NOT_FOUND");
        }

        if (key.IsRevoked())
        {
            return Result.Failure("Cannot update revoked key", "KEY_REVOKED");
        }

        var ipEntry = key.IpWhitelist.FirstOrDefault(ip => ip.Id == whitelistId);
        if (ipEntry is null)
        {
            return Result.Failure("IP whitelist entry not found", "IP_NOT_FOUND");
        }

        key.RemoveIpWhitelistById(whitelistId);

        _unitOfWork.ApiKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<ApiKeyValidationResult> ValidateKeyAsync(
        string plainTextKey,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Validate format
        if (!_keyGenerator.ValidateKeyFormat(plainTextKey))
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorCode = "INVALID_FORMAT",
                ErrorMessage = "Invalid API key format"
            };
        }

        // Hash the key and look it up
        var keyHash = _keyGenerator.HashKey(plainTextKey);
        var key = await _unitOfWork.ApiKeys.GetByHashAsync(keyHash, cancellationToken);

        if (key is null)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorCode = "KEY_NOT_FOUND",
                ErrorMessage = "API key not found"
            };
        }

        // Check if key is valid
        if (!key.IsValid())
        {
            string errorCode;
            string errorMessage;

            if (key.IsRevoked())
            {
                errorCode = "KEY_REVOKED";
                errorMessage = "API key has been revoked";
            }
            else if (key.IsExpired())
            {
                errorCode = "KEY_EXPIRED";
                errorMessage = "API key has expired";
            }
            else
            {
                errorCode = "KEY_INACTIVE";
                errorMessage = "API key is not active";
            }

            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }

        // Check if client is active
        if (!key.ApiClient.IsActive)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorCode = "CLIENT_INACTIVE",
                ErrorMessage = "API client is not active"
            };
        }

        // Check IP whitelist
        if (!key.IsIpAllowed(ipAddress))
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorCode = "IP_NOT_ALLOWED",
                ErrorMessage = "IP address not allowed"
            };
        }

        // Update last used (fire and forget - don't block validation)
        _ = _unitOfWork.ApiKeys.UpdateLastUsedAsync(key.Id, ipAddress, CancellationToken.None);

        return new ApiKeyValidationResult
        {
            IsValid = true,
            ApiKeyId = key.Id,
            ApiClientId = key.ApiClientId,
            ClientName = key.ApiClient.Name,
            Permissions = key.Permissions.Select(p => p.Scope).ToList(),
            RateLimitPerMinute = key.RateLimitPerMinute
        };
    }

    public async Task<Result<PagedResult<ApiKeyUsageLogDto>>> GetUsageLogsAsync(
        UsageLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        if (!filter.ApiKeyId.HasValue)
        {
            return Result<PagedResult<ApiKeyUsageLogDto>>.Failure("API key ID is required", "KEY_ID_REQUIRED");
        }

        var key = await _unitOfWork.ApiKeys.GetByIdAsync(filter.ApiKeyId.Value, cancellationToken);
        if (key is null)
        {
            return Result<PagedResult<ApiKeyUsageLogDto>>.Failure("API key not found", "KEY_NOT_FOUND");
        }

        var logs = await _unitOfWork.ApiKeyUsageLogs.GetLogsAsync(filter, cancellationToken);

        var dtos = _mapper.Map<List<ApiKeyUsageLogDto>>(logs.Items);
        var pagedResult = new PagedResult<ApiKeyUsageLogDto>(
            dtos,
            logs.TotalCount,
            filter.Page,
            filter.PageSize);

        return Result<PagedResult<ApiKeyUsageLogDto>>.Success(pagedResult);
    }

    public async Task<Result<ApiKeyUsageStatsDto>> GetUsageStatsAsync(
        Guid keyId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var key = await _unitOfWork.ApiKeys.GetByIdAsync(keyId, cancellationToken);
        if (key is null)
        {
            return Result<ApiKeyUsageStatsDto>.Failure("API key not found", "KEY_NOT_FOUND");
        }

        var stats = await _unitOfWork.ApiKeyUsageLogs.GetStatsAsync(
            keyId,
            fromDate,
            toDate,
            cancellationToken);

        return Result<ApiKeyUsageStatsDto>.Success(stats);
    }
}
