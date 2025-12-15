using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetSubCategories;

/// <summary>
/// Handler for GetSubCategoriesQuery
/// </summary>
public class GetSubCategoriesQueryHandler : IQueryHandler<GetSubCategoriesQuery, Result<List<CategoryListDto>>>
{
    private readonly ICategoryService _categoryService;

    public GetSubCategoriesQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryListDto>>> Handle(GetSubCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _categoryService.GetSubCategoriesAsync(request.ParentId, request.ActiveOnly, cancellationToken);
    }
}
