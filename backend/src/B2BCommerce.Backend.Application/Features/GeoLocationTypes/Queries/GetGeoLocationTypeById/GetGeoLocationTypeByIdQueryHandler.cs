using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypeById;

/// <summary>
/// Handler for GetGeoLocationTypeByIdQuery
/// </summary>
public class GetGeoLocationTypeByIdQueryHandler : IQueryHandler<GetGeoLocationTypeByIdQuery, Result<GeoLocationTypeDto>>
{
    private readonly IGeoLocationTypeService _service;

    public GetGeoLocationTypeByIdQueryHandler(IGeoLocationTypeService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationTypeDto>> Handle(GetGeoLocationTypeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.Id, cancellationToken);
    }
}
