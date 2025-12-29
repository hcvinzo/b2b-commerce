using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.DeleteGeoLocation;

/// <summary>
/// Handler for DeleteGeoLocationCommand
/// </summary>
public class DeleteGeoLocationCommandHandler : ICommandHandler<DeleteGeoLocationCommand, Result>
{
    private readonly IGeoLocationService _service;

    public DeleteGeoLocationCommandHandler(IGeoLocationService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(DeleteGeoLocationCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(request.Id, cancellationToken);
    }
}
