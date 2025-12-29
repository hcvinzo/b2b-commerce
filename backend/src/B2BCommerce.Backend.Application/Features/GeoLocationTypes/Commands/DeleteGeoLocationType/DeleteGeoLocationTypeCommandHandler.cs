using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.DeleteGeoLocationType;

/// <summary>
/// Handler for DeleteGeoLocationTypeCommand
/// </summary>
public class DeleteGeoLocationTypeCommandHandler : ICommandHandler<DeleteGeoLocationTypeCommand, Result>
{
    private readonly IGeoLocationTypeService _service;

    public DeleteGeoLocationTypeCommandHandler(IGeoLocationTypeService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(DeleteGeoLocationTypeCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(request.Id, cancellationToken);
    }
}
