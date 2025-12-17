using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Common.Helpers;
using B2BCommerce.Backend.Application.DTOs.Brands;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.UpsertBrand;

/// <summary>
/// Handler for UpsertBrandCommand.
/// Creates or updates a brand based on ExternalId (primary) or Name (fallback).
/// </summary>
public class UpsertBrandCommandHandler : ICommandHandler<UpsertBrandCommand, Result<BrandDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpsertBrandCommandHandler> _logger;

    public UpsertBrandCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpsertBrandCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BrandDto>> Handle(UpsertBrandCommand request, CancellationToken cancellationToken)
    {
        // Use shared lookup helper: ExternalId → Name
        var lookup = await ExternalEntityLookupExtensions.LookupExternalEntityAsync(
            request.ExternalId,
            request.Name,  // fallback key
            (extId, ct) => _unitOfWork.Brands.GetByExternalIdAsync(extId, ct),
            (name, ct) => _unitOfWork.Brands.GetByNameAsync(name, ct),
            cancellationToken);

        Brand brand;

        if (lookup.IsNew)
        {
            // If ExternalId provided, use CreateFromExternal; otherwise use Create (auto-generates ExternalId)
            if (!string.IsNullOrEmpty(lookup.EffectiveExternalId))
            {
                brand = Brand.CreateFromExternal(
                    externalId: lookup.EffectiveExternalId,
                    name: request.Name,
                    description: request.Description ?? string.Empty,
                    logoUrl: request.LogoUrl,
                    websiteUrl: request.WebsiteUrl,
                    isActive: request.IsActive,
                    externalCode: request.ExternalCode);
            }
            else
            {
                // No ExternalId provided - create with auto-generated Id and ExternalId
                brand = Brand.Create(request.Name, request.Description ?? string.Empty);
                if (!string.IsNullOrEmpty(request.LogoUrl))
                {
                    brand.SetLogo(request.LogoUrl);
                }
                if (!string.IsNullOrEmpty(request.WebsiteUrl))
                {
                    brand.Update(request.Name, request.Description ?? string.Empty, request.WebsiteUrl);
                }
                if (!request.IsActive)
                {
                    brand.Deactivate();
                }
                // Note: ExternalCode is ignored when creating without ExternalId
                // ExternalId is auto-generated from the internal Id
            }

            await _unitOfWork.Brands.AddAsync(brand, cancellationToken);

            _logger.LogInformation(
                "Creating brand: {ExternalId} - {Name} (Id: {Id})",
                brand.ExternalId, request.Name, brand.Id);
        }
        else
        {
            brand = lookup.Entity!;

            // Update existing brand
            brand.UpdateFromExternal(
                name: request.Name,
                description: request.Description ?? string.Empty,
                logoUrl: request.LogoUrl,
                websiteUrl: request.WebsiteUrl,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            _logger.LogInformation(
                "Updating brand from external sync: {ExternalId} - {Name}",
                brand.ExternalId, request.Name);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        var dto = MapToDto(brand);

        return Result<BrandDto>.Success(dto);
    }

    private static BrandDto MapToDto(Brand brand)
    {
        return new BrandDto
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
    }
}
