using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Queries.GetProductRelations;

/// <summary>
/// Query to get all relations for a product, grouped by type
/// </summary>
public record GetProductRelationsQuery(Guid ProductId) : IQuery<Result<List<ProductRelationsGroupDto>>>;
