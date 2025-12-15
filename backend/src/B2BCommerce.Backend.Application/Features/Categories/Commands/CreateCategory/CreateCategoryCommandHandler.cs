using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Handler for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public CreateCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = new CreateCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        return await _categoryService.CreateAsync(dto, request.CreatedBy, cancellationToken);
    }
}
