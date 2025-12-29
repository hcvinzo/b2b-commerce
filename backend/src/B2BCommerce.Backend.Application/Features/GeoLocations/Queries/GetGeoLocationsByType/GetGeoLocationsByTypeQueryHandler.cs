using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByType;

/// <summary>
/// Handler for GetGeoLocationsByTypeQuery
/// </summary>
public class GetGeoLocationsByTypeQueryHandler : IQueryHandler<GetGeoLocationsByTypeQuery, Result<List<GeoLocationListDto>>>
{
    private readonly IGeoLocationService _service;

    public GetGeoLocationsByTypeQueryHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<List<GeoLocationListDto>>> Handle(GetGeoLocationsByTypeQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByTypeAsync(request.TypeId, cancellationToken);
    }
}
