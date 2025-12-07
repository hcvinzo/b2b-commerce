using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.CreateApiClient;

/// <summary>
/// Handler for CreateApiClientCommand
/// </summary>
public class CreateApiClientCommandHandler : ICommandHandler<CreateApiClientCommand, Result<ApiClientDetailDto>>
{
    private readonly IApiClientService _apiClientService;

    public CreateApiClientCommandHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result<ApiClientDetailDto>> Handle(CreateApiClientCommand request, CancellationToken cancellationToken)
    {
        var dto = new CreateApiClientDto
        {
            Name = request.Name,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        return await _apiClientService.CreateAsync(dto, request.CreatedBy, cancellationToken);
    }
}
