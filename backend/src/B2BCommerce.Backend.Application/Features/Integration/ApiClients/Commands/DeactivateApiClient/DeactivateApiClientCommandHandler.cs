using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeactivateApiClient;

/// <summary>
/// Handler for DeactivateApiClientCommand
/// </summary>
public class DeactivateApiClientCommandHandler : ICommandHandler<DeactivateApiClientCommand, Result>
{
    private readonly IApiClientService _apiClientService;

    public DeactivateApiClientCommandHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result> Handle(DeactivateApiClientCommand request, CancellationToken cancellationToken)
    {
        return await _apiClientService.DeactivateAsync(request.Id, request.UpdatedBy, cancellationToken);
    }
}
