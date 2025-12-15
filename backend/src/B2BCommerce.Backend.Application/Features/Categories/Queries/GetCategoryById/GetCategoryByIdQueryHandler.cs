using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryById;

/// <summary>
/// Handler for GetCategoryByIdQuery
/// </summary>
public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public GetCategoryByIdQueryHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _categoryService.GetByIdAsync(request.Id, cancellationToken);
    }
}
