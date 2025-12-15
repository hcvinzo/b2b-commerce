using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeByCode;

/// <summary>
/// Query to get a product type by its code
/// </summary>
public record GetProductTypeByCodeQuery(string Code) : IQuery<Result<ProductTypeDto>>;
