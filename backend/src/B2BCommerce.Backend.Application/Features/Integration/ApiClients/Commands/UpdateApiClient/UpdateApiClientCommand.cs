using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.UpdateApiClient;

/// <summary>
/// Command to update an existing API client
/// </summary>
public record UpdateApiClientCommand(
    Guid Id,
    string Name,
    string? Description,
    string ContactEmail,
    string? ContactPhone,
    string UpdatedBy) : ICommand<Result<ApiClientDetailDto>>;
