using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionProducts;

/// <summary>
/// Handler for SetCollectionProductsCommand.
/// Sets products for a manual collection (replaces all existing).
/// </summary>
public class SetCollectionProductsCommandHandler : ICommandHandler<SetCollectionProductsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetCollectionProductsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetCollectionProductsCommand request, CancellationToken cancellationToken)
    {
        // Get collection (without products - we'll manage them separately via repository)
        var collection = await _unitOfWork.Collections.GetByIdAsync(request.CollectionId, cancellationToken);
        if (collection is null)
        {
            return Result.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        // Verify this is a manual collection
        if (collection.Type != CollectionType.Manual)
        {
            return Result.Failure("Products can only be set on manual collections", "INVALID_COLLECTION_TYPE");
        }

        // Validate product count
        if (request.Products.Count > Collection.MaxProductsPerManualCollection)
        {
            return Result.Failure(
                $"Cannot add more than {Collection.MaxProductsPerManualCollection} products to a manual collection",
                "MAX_PRODUCTS_EXCEEDED");
        }

        // Validate all products exist
        var productIds = request.Products.Select(p => p.ProductId).Distinct().ToList();
        foreach (var productId in productIds)
        {
            var exists = await _unitOfWork.Products.AnyAsync(p => p.Id == productId, cancellationToken);
            if (!exists)
            {
                return Result.Failure($"Product {productId} not found", "PRODUCT_NOT_FOUND");
            }
        }

        // Use repository method to replace products (handles EF Core tracking correctly)
        await _unitOfWork.Collections.ReplaceProductsAsync(
            request.CollectionId,
            request.Products.Select(p => (p.ProductId, p.DisplayOrder, p.IsFeatured)).ToList(),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
