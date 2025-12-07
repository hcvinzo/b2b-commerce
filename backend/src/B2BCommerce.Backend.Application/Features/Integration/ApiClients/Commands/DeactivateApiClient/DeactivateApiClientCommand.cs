using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeactivateApiClient;

/// <summary>
/// Command to deactivate an API client (also deactivates all keys)
/// </summary>
public record DeactivateApiClientCommand(Guid Id, string UpdatedBy) : ICommand<Result>;
