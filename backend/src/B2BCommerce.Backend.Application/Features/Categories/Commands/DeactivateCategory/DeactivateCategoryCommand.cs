using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.DeactivateCategory;

/// <summary>
/// Command to deactivate a category
/// </summary>
public record DeactivateCategoryCommand(Guid Id, string? UpdatedBy) : ICommand<Result<CategoryDto>>;
