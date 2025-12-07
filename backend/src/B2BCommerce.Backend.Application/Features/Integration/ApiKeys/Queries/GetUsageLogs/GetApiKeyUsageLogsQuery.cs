using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageLogs;

/// <summary>
/// Query to get usage logs for an API key
/// </summary>
public record GetApiKeyUsageLogsQuery(
    Guid ApiKeyId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 50) : IQuery<Result<PagedResult<ApiKeyUsageLogDto>>>;
