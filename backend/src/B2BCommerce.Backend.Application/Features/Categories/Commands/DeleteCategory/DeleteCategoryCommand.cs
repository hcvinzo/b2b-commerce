using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Command to delete a category (soft delete)
/// </summary>
public record DeleteCategoryCommand(Guid Id, string? DeletedBy) : ICommand<Result>;
