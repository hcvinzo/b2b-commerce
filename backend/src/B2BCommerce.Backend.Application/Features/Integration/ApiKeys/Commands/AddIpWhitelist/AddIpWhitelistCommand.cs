using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.AddIpWhitelist;

/// <summary>
/// Command to add an IP address to the whitelist
/// </summary>
public record AddIpWhitelistCommand(
    Guid KeyId,
    string IpAddress,
    string? Description) : ICommand<Result>;
