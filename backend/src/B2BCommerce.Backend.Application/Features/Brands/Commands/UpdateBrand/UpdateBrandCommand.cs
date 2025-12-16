using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.UpdateBrand;

/// <summary>
/// Command to update an existing brand
/// </summary>
public record UpdateBrandCommand(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    bool IsActive,
    string? UpdatedBy) : ICommand<Result<BrandDto>>;
