using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.CreateGeoLocationType;

/// <summary>
/// Handler for CreateGeoLocationTypeCommand
/// </summary>
public class CreateGeoLocationTypeCommandHandler : ICommandHandler<CreateGeoLocationTypeCommand, Result<GeoLocationTypeDto>>
{
    private readonly IGeoLocationTypeService _service;

    public CreateGeoLocationTypeCommandHandler(IGeoLocationTypeService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationTypeDto>> Handle(CreateGeoLocationTypeCommand request, CancellationToken cancellationToken)
    {
        return await _service.CreateAsync(request.Name, request.DisplayOrder, cancellationToken);
    }
}
