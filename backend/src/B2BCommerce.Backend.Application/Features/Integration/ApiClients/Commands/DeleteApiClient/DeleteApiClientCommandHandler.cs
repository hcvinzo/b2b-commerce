using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeleteApiClient;

/// <summary>
/// Handler for DeleteApiClientCommand
/// </summary>
public class DeleteApiClientCommandHandler : ICommandHandler<DeleteApiClientCommand, Result>
{
    private readonly IApiClientService _apiClientService;

    public DeleteApiClientCommandHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result> Handle(DeleteApiClientCommand request, CancellationToken cancellationToken)
    {
        return await _apiClientService.DeleteAsync(request.Id, request.DeletedBy, cancellationToken);
    }
}
