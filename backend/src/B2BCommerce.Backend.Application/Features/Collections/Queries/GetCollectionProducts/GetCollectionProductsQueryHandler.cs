using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollectionProducts;

/// <summary>
/// Handler for GetCollectionProductsQuery.
/// Returns products for both manual and dynamic collections.
/// </summary>
public class GetCollectionProductsQueryHandler
    : IQueryHandler<GetCollectionProductsQuery, Result<PagedResult<ProductInCollectionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCollectionProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ProductInCollectionDto>>> Handle(
        GetCollectionProductsQuery request,
        CancellationToken cancellationToken)
    {
        var collection = await _unitOfWork.Collections.GetWithFilterAsync(request.CollectionId, cancellationToken);
        if (collection is null)
        {
            return Result<PagedResult<ProductInCollectionDto>>.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        return collection.Type == CollectionType.Manual
            ? await GetManualCollectionProducts(request, cancellationToken)
            : await GetDynamicCollectionProducts(request, collection.Filter, cancellationToken);
    }

    private async Task<Result<PagedResult<ProductInCollectionDto>>> GetManualCollectionProducts(
        GetCollectionProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Collections.GetManualCollectionProductsPagedAsync(
            request.CollectionId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(item => new ProductInCollectionDto
        {
            ProductId = item.Product.Id,
            ProductName = item.Product.Name,
            ProductSku = item.Product.SKU,
            ProductImageUrl = item.Product.MainImageUrl,
            ProductPrice = item.Product.ListPrice.Amount,
            ProductIsActive = item.Product.IsActive,
            DisplayOrder = item.ProductCollection.DisplayOrder,
            IsFeatured = item.ProductCollection.IsFeatured
        }).ToList();

        var pagedResult = new PagedResult<ProductInCollectionDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<ProductInCollectionDto>>.Success(pagedResult);
    }

    private async Task<Result<PagedResult<ProductInCollectionDto>>> GetDynamicCollectionProducts(
        GetCollectionProductsQuery request,
        Domain.Entities.CollectionFilter? filter,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Collections.GetDynamicCollectionProductsPagedAsync(
            filter,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(p => new ProductInCollectionDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ProductSku = p.SKU,
            ProductImageUrl = p.MainImageUrl,
            ProductPrice = p.ListPrice.Amount,
            ProductIsActive = p.IsActive,
            DisplayOrder = 0, // No display order for dynamic collections
            IsFeatured = false // No featured status for dynamic collections
        }).ToList();

        var pagedResult = new PagedResult<ProductInCollectionDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<ProductInCollectionDto>>.Success(pagedResult);
    }
}
