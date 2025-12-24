using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductRelations;

/// <summary>
/// Handler for GetProductRelationsQuery
/// </summary>
public class GetProductRelationsQueryHandler
    : IQueryHandler<GetProductRelationsQuery, Result<List<ProductRelationsGroupDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductRelationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ProductRelationsGroupDto>>> Handle(
        GetProductRelationsQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetWithRelationsAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<List<ProductRelationsGroupDto>>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        var groups = new List<ProductRelationsGroupDto>();

        foreach (ProductRelationType relationType in Enum.GetValues<ProductRelationType>())
        {
            var relations = product.SourceRelations
                .Where(r => r.RelationType == relationType)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new ProductRelationDto
                {
                    Id = r.Id,
                    RelatedProductId = r.RelatedProductId,
                    RelatedProductName = r.RelatedProduct?.Name ?? string.Empty,
                    RelatedProductSku = r.RelatedProduct?.SKU ?? string.Empty,
                    RelatedProductImageUrl = r.RelatedProduct?.MainImageUrl,
                    RelatedProductPrice = r.RelatedProduct?.ListPrice.Amount ?? 0,
                    RelatedProductIsActive = r.RelatedProduct?.IsActive ?? false,
                    RelationType = r.RelationType,
                    DisplayOrder = r.DisplayOrder
                })
                .ToList();

            groups.Add(new ProductRelationsGroupDto
            {
                RelationType = relationType,
                RelationTypeName = GetRelationTypeName(relationType),
                Relations = relations
            });
        }

        return Result<List<ProductRelationsGroupDto>>.Success(groups);
    }

    private static string GetRelationTypeName(ProductRelationType relationType)
    {
        return relationType switch
        {
            ProductRelationType.Related => "Related Products",
            ProductRelationType.CrossSell => "Cross-sell",
            ProductRelationType.UpSell => "Up-sell",
            ProductRelationType.Accessories => "Accessories",
            _ => relationType.ToString()
        };
    }
}
