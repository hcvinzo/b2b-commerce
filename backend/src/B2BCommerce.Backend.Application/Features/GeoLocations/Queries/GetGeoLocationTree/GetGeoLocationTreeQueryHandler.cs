using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationTree;

/// <summary>
/// Handler for GetGeoLocationTreeQuery
/// </summary>
public class GetGeoLocationTreeQueryHandler : IQueryHandler<GetGeoLocationTreeQuery, Result<List<GeoLocationTreeDto>>>
{
    private readonly IGeoLocationService _service;

    public GetGeoLocationTreeQueryHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<List<GeoLocationTreeDto>>> Handle(GetGeoLocationTreeQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetTreeAsync(request.TypeId, cancellationToken);
    }
}
