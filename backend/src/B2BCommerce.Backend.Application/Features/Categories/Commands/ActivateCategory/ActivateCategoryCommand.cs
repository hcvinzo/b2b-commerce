using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.ActivateCategory;

/// <summary>
/// Command to activate a category
/// </summary>
public record ActivateCategoryCommand(Guid Id, string? UpdatedBy) : ICommand<Result<CategoryDto>>;
