using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.DeactivateCategory;

/// <summary>
/// Handler for DeactivateCategoryCommand
/// </summary>
public class DeactivateCategoryCommandHandler : ICommandHandler<DeactivateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public DeactivateCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<CategoryDto>> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        return await _categoryService.DeactivateAsync(request.Id, request.UpdatedBy, cancellationToken);
    }
}
