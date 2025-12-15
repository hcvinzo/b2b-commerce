using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Command to update an existing category
/// </summary>
public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    string? UpdatedBy) : ICommand<Result<CategoryDto>>;
