using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Queries.GetBrands;

/// <summary>
/// Query to get brands with pagination and filtering
/// </summary>
public record GetBrandsQuery(
    string? Search,
    bool? IsActive,
    int PageNumber,
    int PageSize,
    string SortBy,
    string SortDirection) : IQuery<Result<PagedResult<BrandListDto>>>;
