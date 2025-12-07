using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.UpdateApiClient;

/// <summary>
/// Handler for UpdateApiClientCommand
/// </summary>
public class UpdateApiClientCommandHandler : ICommandHandler<UpdateApiClientCommand, Result<ApiClientDetailDto>>
{
    private readonly IApiClientService _apiClientService;

    public UpdateApiClientCommandHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result<ApiClientDetailDto>> Handle(UpdateApiClientCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdateApiClientDto
        {
            Name = request.Name,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        return await _apiClientService.UpdateAsync(request.Id, dto, request.UpdatedBy, cancellationToken);
    }
}
