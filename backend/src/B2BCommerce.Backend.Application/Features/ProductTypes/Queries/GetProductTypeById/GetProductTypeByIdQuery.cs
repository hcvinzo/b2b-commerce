using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeById;

/// <summary>
/// Query to get a product type by its ID
/// </summary>
public record GetProductTypeByIdQuery(Guid Id) : IQuery<Result<ProductTypeDto>>;
