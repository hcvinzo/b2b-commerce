using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.CreateGeoLocation;

/// <summary>
/// Handler for CreateGeoLocationCommand
/// </summary>
public class CreateGeoLocationCommandHandler : ICommandHandler<CreateGeoLocationCommand, Result<GeoLocationDto>>
{
    private readonly IGeoLocationService _service;

    public CreateGeoLocationCommandHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationDto>> Handle(CreateGeoLocationCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(
            request.GeoLocationTypeId,
            request.Code,
            request.Name,
            request.ParentId,
            request.Latitude,
            request.Longitude,
            request.Metadata,
            cancellationToken);
    }
}
