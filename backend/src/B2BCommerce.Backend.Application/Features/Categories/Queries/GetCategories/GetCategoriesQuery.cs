using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Query to get paginated categories with filtering
/// </summary>
public record GetCategoriesQuery(
    string? Search,
    Guid? ParentCategoryId,
    bool? IsActive,
    int PageNumber,
    int PageSize,
    string SortBy,
    string SortDirection) : IQuery<Result<PagedResult<CategoryListDto>>>;
