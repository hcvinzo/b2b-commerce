using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.Infrastructure.Services.Integration;

/// <summary>
/// Service implementation for API client management
/// </summary>
public class ApiClientService : IApiClientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ApiClientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ApiClientDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id, cancellationToken);

        if (client is null)
        {
            return Result<ApiClientDetailDto>.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        var dto = _mapper.Map<ApiClientDetailDto>(client);
        return Result<ApiClientDetailDto>.Success(dto);
    }

    public async Task<Result<PagedResult<ApiClientListDto>>> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // Use the repository methods instead of Query()
        var allClients = (await _unitOfWork.ApiClients.GetAllAsync(cancellationToken)).ToList();

        // Filter by isActive if provided
        var filteredClients = isActive.HasValue
            ? allClients.Where(x => x.IsActive == isActive.Value).ToList()
            : allClients;

        var totalCount = filteredClients.Count;

        var pagedClients = filteredClients
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<List<ApiClientListDto>>(pagedClients);
        var pagedResult = new PagedResult<ApiClientListDto>(dtos, totalCount, page, pageSize);

        return Result<PagedResult<ApiClientListDto>>.Success(pagedResult);
    }

    public async Task<Result<ApiClientDetailDto>> CreateAsync(
        CreateApiClientDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        var isUnique = await _unitOfWork.ApiClients.IsNameUniqueAsync(dto.Name, cancellationToken: cancellationToken);
        if (!isUnique)
        {
            return Result<ApiClientDetailDto>.Failure("A client with this name already exists", "DUPLICATE_NAME");
        }

        var client = new ApiClient(
            dto.Name,
            dto.ContactEmail,
            dto.Description,
            dto.ContactPhone);

        client.CreatedBy = createdBy;

        await _unitOfWork.ApiClients.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<ApiClientDetailDto>(client);
        return Result<ApiClientDetailDto>.Success(resultDto);
    }

    public async Task<Result<ApiClientDetailDto>> UpdateAsync(
        Guid id,
        UpdateApiClientDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return Result<ApiClientDetailDto>.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        // Check for duplicate name if name is changing
        if (!string.Equals(client.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isUnique = await _unitOfWork.ApiClients.IsNameUniqueAsync(dto.Name, id, cancellationToken);
            if (!isUnique)
            {
                return Result<ApiClientDetailDto>.Failure("A client with this name already exists", "DUPLICATE_NAME");
            }
        }

        client.Update(
            dto.Name,
            dto.ContactEmail,
            dto.Description,
            dto.ContactPhone);

        client.UpdatedBy = updatedBy;

        _unitOfWork.ApiClients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<ApiClientDetailDto>(client);
        return Result<ApiClientDetailDto>.Success(resultDto);
    }

    public async Task<Result> ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return Result.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        if (client.IsActive)
        {
            return Result.Failure("Client is already active", "ALREADY_ACTIVE");
        }

        client.Activate();
        client.UpdatedBy = updatedBy;

        _unitOfWork.ApiClients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id, cancellationToken);

        if (client is null)
        {
            return Result.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        if (!client.IsActive)
        {
            return Result.Failure("Client is already inactive", "ALREADY_INACTIVE");
        }

        client.Deactivate(updatedBy);
        client.UpdatedBy = updatedBy;

        _unitOfWork.ApiClients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id, cancellationToken);

        if (client is null)
        {
            return Result.Failure("API client not found", "CLIENT_NOT_FOUND");
        }

        // Check if client has active keys
        var hasActiveKeys = client.ApiKeys.Any(k => k.IsActive && !k.IsExpired() && !k.IsRevoked());
        if (hasActiveKeys)
        {
            return Result.Failure("Cannot delete client with active API keys. Revoke all keys first.", "HAS_ACTIVE_KEYS");
        }

        // Soft delete using BaseEntity properties
        client.IsDeleted = true;
        client.DeletedAt = DateTime.UtcNow;
        client.DeletedBy = deletedBy;

        _unitOfWork.ApiClients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
