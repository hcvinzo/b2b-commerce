using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.SetProductRelations;

/// <summary>
/// Handler for SetProductRelationsCommand.
/// Implements bidirectional relationships - when A relates to B, B also relates to A.
/// </summary>
public class SetProductRelationsCommandHandler : ICommandHandler<SetProductRelationsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetProductRelationsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetProductRelationsCommand request, CancellationToken cancellationToken)
    {
        // Validate count limit
        if (request.RelatedProducts.Count > ProductRelation.MaxRelatedProductsPerType)
        {
            return Result.Failure(
                $"Cannot add more than {ProductRelation.MaxRelatedProductsPerType} related products per type",
                "MAX_RELATIONS_EXCEEDED");
        }

        // Get source product with existing relations
        var product = await _unitOfWork.Products.GetWithRelationsAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        // Validate all related products exist and check for self-relation
        var relatedProductIds = request.RelatedProducts.Select(rp => rp.ProductId).Distinct().ToList();
        if (relatedProductIds.Contains(request.ProductId))
        {
            return Result.Failure("A product cannot be related to itself", "SELF_RELATION_NOT_ALLOWED");
        }

        foreach (var relatedId in relatedProductIds)
        {
            var exists = await _unitOfWork.Products.AnyAsync(p => p.Id == relatedId, cancellationToken);
            if (!exists)
            {
                return Result.Failure($"Related product {relatedId} not found", "RELATED_PRODUCT_NOT_FOUND");
            }
        }

        // Get existing relation IDs to remove reverse relations later
        var existingRelatedIds = product.GetRelationsOfType(request.RelationType)
            .Select(r => r.RelatedProductId)
            .ToList();

        // Remove existing relations of this type from source product
        product.ClearRelationsOfType(request.RelationType);

        // Remove reverse relations from previously related products
        foreach (var oldRelatedId in existingRelatedIds)
        {
            var oldRelatedProduct = await _unitOfWork.Products.GetWithRelationsAsync(oldRelatedId, cancellationToken);
            if (oldRelatedProduct is not null)
            {
                oldRelatedProduct.RemoveRelatedProduct(request.ProductId, request.RelationType);
            }
        }

        // Add new relations (bidirectional)
        foreach (var input in request.RelatedProducts)
        {
            // Add forward relation
            product.AddRelatedProduct(input.ProductId, request.RelationType, input.DisplayOrder);

            // Add reverse relation
            var relatedProduct = await _unitOfWork.Products.GetWithRelationsAsync(input.ProductId, cancellationToken);
            if (relatedProduct is not null)
            {
                // Check if reverse already exists (shouldn't happen in clean state)
                if (!relatedProduct.HasRelation(request.ProductId, request.RelationType))
                {
                    relatedProduct.AddRelatedProduct(request.ProductId, request.RelationType, input.DisplayOrder);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
