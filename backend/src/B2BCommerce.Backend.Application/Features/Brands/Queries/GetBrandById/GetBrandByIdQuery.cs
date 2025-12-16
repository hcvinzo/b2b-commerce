using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Queries.GetBrandById;

/// <summary>
/// Query to get a brand by ID
/// </summary>
public record GetBrandByIdQuery(Guid Id) : IQuery<Result<BrandDto>>;
