using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductBySku;

/// <summary>
/// Query to get a product by its SKU
/// </summary>
public record GetProductBySkuQuery(string SKU) : IQuery<Result<ProductDto>>;
