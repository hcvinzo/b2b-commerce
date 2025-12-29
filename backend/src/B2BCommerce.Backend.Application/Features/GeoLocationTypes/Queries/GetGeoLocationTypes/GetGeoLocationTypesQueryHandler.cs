using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypes;

/// <summary>
/// Handler for GetGeoLocationTypesQuery
/// </summary>
public class GetGeoLocationTypesQueryHandler : IQueryHandler<GetGeoLocationTypesQuery, Result<List<GeoLocationTypeDto>>>
{
    private readonly IGeoLocationTypeService _service;

    public GetGeoLocationTypesQueryHandler(IGeoLocationTypeService service)
    {
        _service = service;
    }

    public async Task<Result<List<GeoLocationTypeDto>>> Handle(GetGeoLocationTypesQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetAllAsync(cancellationToken);
    }
}
