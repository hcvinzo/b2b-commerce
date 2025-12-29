using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocations;

/// <summary>
/// Handler for GetGeoLocationsQuery
/// </summary>
public class GetGeoLocationsQueryHandler : IQueryHandler<GetGeoLocationsQuery, Result<PagedResult<GeoLocationListDto>>>
{
    private readonly IGeoLocationService _service;

    public GetGeoLocationsQueryHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<PagedResult<GeoLocationListDto>>> Handle(GetGeoLocationsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetAllAsync(
            request.Search,
            request.TypeId,
            request.ParentId,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);
    }
}
