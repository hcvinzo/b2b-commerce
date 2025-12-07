using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.CreateApiClient;

/// <summary>
/// Command to create a new API client
/// </summary>
public record CreateApiClientCommand(
    string Name,
    string? Description,
    string ContactEmail,
    string? ContactPhone,
    string CreatedBy) : ICommand<Result<ApiClientDetailDto>>;
