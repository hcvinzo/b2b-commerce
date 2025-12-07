using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeyById;

/// <summary>
/// Query to get an API key by ID
/// </summary>
public record GetApiKeyByIdQuery(Guid Id) : IQuery<Result<ApiKeyDetailDto>>;
