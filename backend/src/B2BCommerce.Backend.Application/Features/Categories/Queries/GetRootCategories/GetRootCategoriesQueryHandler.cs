using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetRootCategories;

/// <summary>
/// Handler for GetRootCategoriesQuery
/// </summary>
public class GetRootCategoriesQueryHandler : IQueryHandler<GetRootCategoriesQuery, Result<List<CategoryListDto>>>
{
    private readonly ICategoryService _categoryService;

    public GetRootCategoriesQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryListDto>>> Handle(GetRootCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _categoryService.GetRootCategoriesAsync(request.ActiveOnly, cancellationToken);
    }
}
