using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeysByClient;

/// <summary>
/// Query to get all API keys for a client
/// </summary>
public record GetApiKeysByClientQuery(Guid ClientId) : IQuery<Result<List<ApiKeyListDto>>>;
