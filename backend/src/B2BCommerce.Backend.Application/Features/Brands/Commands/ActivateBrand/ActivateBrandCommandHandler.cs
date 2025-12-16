using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.ActivateBrand;

/// <summary>
/// Handler for ActivateBrandCommand
/// </summary>
public class ActivateBrandCommandHandler : ICommandHandler<ActivateBrandCommand, Result<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandDto>> Handle(ActivateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.Id, cancellationToken);

        if (brand is null)
        {
            return Result<BrandDto>.Failure("Brand not found", "BRAND_NOT_FOUND");
        }

        brand.Activate();
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
