using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Query to get products by category
/// </summary>
public record GetProductsByCategoryQuery(Guid CategoryId) : IQuery<Result<IEnumerable<ProductListDto>>>;
