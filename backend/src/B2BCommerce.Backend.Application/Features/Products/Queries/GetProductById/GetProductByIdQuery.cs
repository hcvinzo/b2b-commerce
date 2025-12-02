using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to get a product by its ID
/// </summary>
public record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto>>;
