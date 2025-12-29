using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationById;

/// <summary>
/// Handler for GetGeoLocationByIdQuery
/// </summary>
public class GetGeoLocationByIdQueryHandler : IQueryHandler<GetGeoLocationByIdQuery, Result<GeoLocationDto>>
{
    private readonly IGeoLocationService _service;

    public GetGeoLocationByIdQueryHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationDto>> Handle(GetGeoLocationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(request.Id, cancellationToken);
    }
}
