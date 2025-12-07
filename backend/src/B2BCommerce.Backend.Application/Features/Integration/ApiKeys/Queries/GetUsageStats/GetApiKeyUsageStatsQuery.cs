using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageStats;

/// <summary>
/// Query to get usage statistics for an API key
/// </summary>
public record GetApiKeyUsageStatsQuery(
    Guid ApiKeyId,
    DateTime FromDate,
    DateTime ToDate) : IQuery<Result<ApiKeyUsageStatsDto>>;
