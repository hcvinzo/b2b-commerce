using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.UpdateGeoLocationType;

/// <summary>
/// Handler for UpdateGeoLocationTypeCommand
/// </summary>
public class UpdateGeoLocationTypeCommandHandler : ICommandHandler<UpdateGeoLocationTypeCommand, Result<GeoLocationTypeDto>>
{
    private readonly IGeoLocationTypeService _service;

    public UpdateGeoLocationTypeCommandHandler(IGeoLocationTypeService service)
    {
        _service = service;
    }

    public async Task<Result<GeoLocationTypeDto>> Handle(UpdateGeoLocationTypeCommand request, CancellationToken cancellationToken)
    {
        return await _service.UpdateAsync(request.Id, request.Name, request.DisplayOrder, cancellationToken);
    }
}
