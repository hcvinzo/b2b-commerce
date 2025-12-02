using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Handler for GetProductsByCategoryQuery
/// </summary>
public class GetProductsByCategoryQueryHandler : IQueryHandler<GetProductsByCategoryQuery, Result<IEnumerable<ProductListDto>>>
{
    private readonly IProductService _productService;

    public GetProductsByCategoryQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<IEnumerable<ProductListDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetByCategoryAsync(request.CategoryId, cancellationToken);
    }
}
