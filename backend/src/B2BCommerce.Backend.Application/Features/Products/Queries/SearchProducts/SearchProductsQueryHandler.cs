using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.SearchProducts;

/// <summary>
/// Handler for SearchProductsQuery
/// </summary>
public class SearchProductsQueryHandler : IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductListDto>>>
{
    private readonly IProductService _productService;

    public SearchProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<PagedResult<ProductListDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productService.SearchAsync(request.SearchTerm, request.PageNumber, request.PageSize, cancellationToken);
    }
}
