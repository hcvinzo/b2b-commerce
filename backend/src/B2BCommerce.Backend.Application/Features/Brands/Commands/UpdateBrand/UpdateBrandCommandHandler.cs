using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.UpdateBrand;

/// <summary>
/// Handler for UpdateBrandCommand
/// </summary>
public class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand, Result<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.Id, cancellationToken);

        if (brand is null)
        {
            return Result<BrandDto>.Failure("Brand not found", "BRAND_NOT_FOUND");
        }

        // Check if another brand with same name already exists
        var existingBrands = await _unitOfWork.Brands.GetAllAsync(cancellationToken);
        if (existingBrands.Any(b => b.Id != request.Id && b.Name.ToLower() == request.Name.ToLower()))
        {
            return Result<BrandDto>.Failure("A brand with this name already exists", "BRAND_NAME_EXISTS");
        }

        brand.Update(request.Name, request.Description ?? string.Empty, request.WebsiteUrl);

        if (!string.IsNullOrEmpty(request.LogoUrl))
        {
            brand.SetLogo(request.LogoUrl);
        }

        if (request.IsActive)
        {
            brand.Activate();
        }
        else
        {
            brand.Deactivate();
        }

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            LogoUrl = brand.LogoUrl,
            WebsiteUrl = brand.WebsiteUrl,
            IsActive = brand.IsActive,
            ExternalId = brand.ExternalId,
            ExternalCode = brand.ExternalCode,
            LastSyncedAt = brand.LastSyncedAt,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };

        return Result<BrandDto>.Success(dto);
    }
}
