using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.CreateBrand;

/// <summary>
/// Command to create a new brand
/// </summary>
public record CreateBrandCommand(
    string Name,
    string? Description,
    string? LogoUrl,
    string? WebsiteUrl,
    bool IsActive,
    string? CreatedBy) : ICommand<Result<BrandDto>>;
