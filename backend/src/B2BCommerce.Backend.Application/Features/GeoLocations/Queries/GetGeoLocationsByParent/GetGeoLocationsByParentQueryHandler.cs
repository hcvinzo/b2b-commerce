using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByParent;

/// <summary>
/// Handler for GetGeoLocationsByParentQuery
/// </summary>
public class GetGeoLocationsByParentQueryHandler : IQueryHandler<GetGeoLocationsByParentQuery, Result<List<GeoLocationListDto>>>
{
    private readonly IGeoLocationService _service;

    public GetGeoLocationsByParentQueryHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<List<GeoLocationListDto>>> Handle(GetGeoLocationsByParentQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByParentAsync(request.ParentId, cancellationToken);
    }
}
