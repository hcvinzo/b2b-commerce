using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for GeoLocation operations
/// </summary>
public interface IGeoLocationService
{
    Task<Result<PagedResult<GeoLocationListDto>>> GetAllAsync(
        string? search,
        Guid? typeId,
        Guid? parentId,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default);

    Task<Result<GeoLocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<GeoLocationTreeDto>>> GetTreeAsync(Guid? typeId = null, CancellationToken cancellationToken = default);

    Task<Result<List<GeoLocationListDto>>> GetByTypeAsync(Guid typeId, CancellationToken cancellationToken = default);

    Task<Result<List<GeoLocationListDto>>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default);

    Task<Result<GeoLocationDto>> CreateAsync(
        Guid geoLocationTypeId,
        string code,
        string name,
        Guid? parentId,
        decimal? latitude,
        decimal? longitude,
        string? metadata,
        CancellationToken cancellationToken = default);

    Task<Result<GeoLocationDto>> UpdateAsync(
        Guid id,
        string code,
        string name,
        decimal? latitude,
        decimal? longitude,
        string? metadata,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
