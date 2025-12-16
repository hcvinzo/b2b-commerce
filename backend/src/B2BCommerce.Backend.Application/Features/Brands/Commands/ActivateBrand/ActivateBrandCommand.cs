using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.ActivateBrand;

/// <summary>
/// Command to activate a brand
/// </summary>
public record ActivateBrandCommand(Guid Id, string? UpdatedBy) : ICommand<Result<BrandDto>>;
