using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.CreateBrand;

/// <summary>
/// Handler for CreateBrandCommand
/// </summary>
public class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, Result<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        // Check if brand with same name already exists
        var existingBrands = await _unitOfWork.Brands.GetAllAsync(cancellationToken);
        if (existingBrands.Any(b => b.Name.ToLower() == request.Name.ToLower()))
        {
            return Result<BrandDto>.Failure("A brand with this name already exists", "BRAND_NAME_EXISTS");
        }

        var brand = Brand.Create(request.Name, request.Description ?? string.Empty);

        if (!string.IsNullOrEmpty(request.LogoUrl))
        {
            brand.SetLogo(request.LogoUrl);
        }

        if (!string.IsNullOrEmpty(request.WebsiteUrl))
        {
            brand.Update(brand.Name, brand.Description, request.WebsiteUrl);
        }

        if (!request.IsActive)
        {
            brand.Deactivate();
        }

        await _unitOfWork.Brands.AddAsync(brand, cancellationToken);
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
