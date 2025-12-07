using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RemoveIpWhitelist;

/// <summary>
/// Command to remove an IP address from the whitelist
/// </summary>
public record RemoveIpWhitelistCommand(
    Guid KeyId,
    Guid WhitelistId) : ICommand<Result>;
