using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.AddIpWhitelist;

/// <summary>
/// Handler for AddIpWhitelistCommand
/// </summary>
public class AddIpWhitelistCommandHandler : ICommandHandler<AddIpWhitelistCommand, Result>
{
    private readonly IApiKeyService _apiKeyService;

    public AddIpWhitelistCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result> Handle(AddIpWhitelistCommand request, CancellationToken cancellationToken)
    {
        var dto = new AddIpWhitelistDto
        {
            IpAddress = request.IpAddress,
            Description = request.Description
        };

        return await _apiKeyService.AddIpToWhitelistAsync(request.KeyId, dto, cancellationToken);
    }
}
