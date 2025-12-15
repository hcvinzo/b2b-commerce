using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.DeleteProductType;

/// <summary>
/// Handler for DeleteProductTypeCommand
/// </summary>
public class DeleteProductTypeCommandHandler : ICommandHandler<DeleteProductTypeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductTypeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProductTypeCommand request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetByIdAsync(request.Id, cancellationToken);
        if (productType == null)
        {
            return Result<bool>.Failure($"Product type with ID '{request.Id}' not found.");
        }

        // Soft delete
        productType.MarkAsDeleted();
        _unitOfWork.ProductTypes.Update(productType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
