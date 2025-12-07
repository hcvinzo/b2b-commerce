using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.ActivateApiClient;

/// <summary>
/// Handler for ActivateApiClientCommand
/// </summary>
public class ActivateApiClientCommandHandler : ICommandHandler<ActivateApiClientCommand, Result>
{
    private readonly IApiClientService _apiClientService;

    public ActivateApiClientCommandHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result> Handle(ActivateApiClientCommand request, CancellationToken cancellationToken)
    {
        return await _apiClientService.ActivateAsync(request.Id, request.UpdatedBy, cancellationToken);
    }
}
