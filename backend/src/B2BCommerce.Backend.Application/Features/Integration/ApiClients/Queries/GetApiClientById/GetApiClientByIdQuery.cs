using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetApiClientById;

/// <summary>
/// Query to get an API client by ID
/// </summary>
public record GetApiClientByIdQuery(Guid Id) : IQuery<Result<ApiClientDetailDto>>;
