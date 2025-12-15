using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Handler for DeleteCategoryCommand
/// </summary>
public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryService _categoryService;

    public DeleteCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        return await _categoryService.DeleteAsync(request.Id, request.DeletedBy, cancellationToken);
    }
}
