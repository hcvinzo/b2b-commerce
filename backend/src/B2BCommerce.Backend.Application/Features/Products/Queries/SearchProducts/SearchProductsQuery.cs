using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.SearchProducts;

/// <summary>
/// Query to search products
/// </summary>
public record SearchProductsQuery(string SearchTerm, int PageNumber = 1, int PageSize = 10) : IQuery<Result<PagedResult<ProductListDto>>>;
