using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetAllProducts;

/// <summary>
/// Handler for GetAllProductsQuery
/// </summary>
public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, Result<PagedResult<ProductListDto>>>
{
    private readonly IProductService _productService;

    public GetAllProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<PagedResult<ProductListDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
