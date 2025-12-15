using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    string? CreatedBy) : ICommand<Result<CategoryDto>>;
