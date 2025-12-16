using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.DeactivateBrand;

/// <summary>
/// Command to deactivate a brand
/// </summary>
public record DeactivateBrandCommand(Guid Id, string? UpdatedBy) : ICommand<Result<BrandDto>>;
