using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.DeleteBrand;

/// <summary>
/// Handler for DeleteBrandCommand
/// </summary>
public class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.Id, cancellationToken);

        if (brand is null)
        {
            return Result.Failure("Brand not found", "BRAND_NOT_FOUND");
        }

        // Check if brand has products
        if (brand.Products?.Any() == true)
        {
            return Result.Failure("Cannot delete brand with associated products", "BRAND_HAS_PRODUCTS");
        }

        _unitOfWork.Brands.Remove(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
