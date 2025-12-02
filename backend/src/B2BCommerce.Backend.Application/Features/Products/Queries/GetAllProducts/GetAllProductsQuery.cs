using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetAllProducts;

/// <summary>
/// Query to get all products with pagination
/// </summary>
public record GetAllProductsQuery(int PageNumber = 1, int PageSize = 10) : IQuery<Result<PagedResult<ProductListDto>>>;
