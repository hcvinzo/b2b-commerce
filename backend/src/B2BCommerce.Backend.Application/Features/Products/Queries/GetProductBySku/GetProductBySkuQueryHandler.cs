using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductBySku;

/// <summary>
/// Handler for GetProductBySkuQuery
/// </summary>
public class GetProductBySkuQueryHandler : IQueryHandler<GetProductBySkuQuery, Result<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductBySkuQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<ProductDto>> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetBySKUAsync(request.SKU, cancellationToken);
    }
}
