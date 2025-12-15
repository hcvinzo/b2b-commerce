using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryTree;

/// <summary>
/// Handler for GetCategoryTreeQuery
/// </summary>
public class GetCategoryTreeQueryHandler : IQueryHandler<GetCategoryTreeQuery, Result<List<CategoryTreeDto>>>
{
    private readonly ICategoryService _categoryService;

    public GetCategoryTreeQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<List<CategoryTreeDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        return await _categoryService.GetCategoryTreeAsync(request.ActiveOnly, cancellationToken);
    }
}
