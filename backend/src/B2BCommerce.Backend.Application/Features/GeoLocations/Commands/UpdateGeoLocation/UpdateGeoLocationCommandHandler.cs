using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.UpdateGeoLocation;

/// <summary>
/// Handler for UpdateGeoLocationCommand
/// </summary>
public class UpdateGeoLocationCommandHandler : ICommandHandler<UpdateGeoLocationCommand, Result<GeoLocationDto>>
{
    private readonly IGeoLocationService _service;

    public UpdateGeoLocationCommandHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationDto>> Handle(UpdateGeoLocationCommand request, CancellationToken cancellationToken)
    {
        return await _service.UpdateAsync(
            request.Id,
            request.Code,
            request.Name,
            request.Latitude,
            request.Longitude,
            request.Metadata,
            cancellationToken);
    }
}
