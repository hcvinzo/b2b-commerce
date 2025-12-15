using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.ActivateCategory;

/// <summary>
/// Handler for ActivateCategoryCommand
/// </summary>
public class ActivateCategoryCommandHandler : ICommandHandler<ActivateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public ActivateCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<CategoryDto>> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        return await _categoryService.ActivateAsync(request.Id, request.UpdatedBy, cancellationToken);
    }
}
