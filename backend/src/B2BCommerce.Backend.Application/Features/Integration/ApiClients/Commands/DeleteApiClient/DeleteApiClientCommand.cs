using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeleteApiClient;

/// <summary>
/// Command to soft delete an API client
/// </summary>
public record DeleteApiClientCommand(Guid Id, string DeletedBy) : ICommand<Result>;
