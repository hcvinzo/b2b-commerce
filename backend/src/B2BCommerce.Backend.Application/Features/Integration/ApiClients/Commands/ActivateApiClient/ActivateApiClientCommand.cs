using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.ActivateApiClient;

/// <summary>
/// Command to activate an API client
/// </summary>
public record ActivateApiClientCommand(Guid Id, string UpdatedBy) : ICommand<Result>;
