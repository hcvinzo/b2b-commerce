using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Handler for GetCategoriesQuery
/// </summary>
public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, Result<PagedResult<CategoryListDto>>>
{
    private readonly ICategoryService _categoryService;

    public GetCategoriesQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<PagedResult<CategoryListDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _categoryService.GetAllAsync(
            request.Search,
            request.ParentCategoryId,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);
    }
}
