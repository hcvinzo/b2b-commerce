using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;

/// <summary>
/// Query to get all product types with optional filtering
/// </summary>
public record GetProductTypesQuery(bool? IsActive = null) : IQuery<Result<IEnumerable<ProductTypeListDto>>>;
