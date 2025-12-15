using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Handler for UpdateCategoryCommand
/// </summary>
public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public UpdateCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdateCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        return await _categoryService.UpdateAsync(request.Id, dto, request.UpdatedBy, cancellationToken);
    }
}
