using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for API client management operations
/// </summary>
public interface IApiClientService
{
    /// <summary>
    /// Gets a client by ID with details
    /// </summary>
    Task<Result<ApiClientDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all clients with pagination
    /// </summary>
    Task<Result<PagedResult<ApiClientListDto>>> GetAllAsync(int page, int pageSize, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new client
    /// </summary>
    Task<Result<ApiClientDetailDto>> CreateAsync(CreateApiClientDto dto, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing client
    /// </summary>
    Task<Result<ApiClientDetailDto>> UpdateAsync(Guid id, UpdateApiClientDto dto, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a client
    /// </summary>
    Task<Result> ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a client (also deactivates all keys)
    /// </summary>
    Task<Result> DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a client
    /// </summary>
    Task<Result> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
}
