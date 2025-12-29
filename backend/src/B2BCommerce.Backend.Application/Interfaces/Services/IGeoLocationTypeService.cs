using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for GeoLocationType operations
/// </summary>
public interface IGeoLocationTypeService
{
    Task<Result<List<GeoLocationTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<GeoLocationTypeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<GeoLocationTypeDto>> CreateAsync(string name, int displayOrder, CancellationToken cancellationToken = default);
    Task<Result<GeoLocationTypeDto>> UpdateAsync(Guid id, string name, int displayOrder, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
