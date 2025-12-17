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
/// Creates or updates a brand based on ExternalId (primary), Id (fallback), or Name (fallback).
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
        // Use shared lookup helper: Id → ExternalId → Name
        var lookup = await ExternalEntityLookupExtensions.LookupExternalEntityAsync(
            request.Id,
            request.ExternalId,
            request.Name,  // fallback key
            (id, ct) => _unitOfWork.Brands.GetByIdAsync(id, ct),
            (extId, ct) => _unitOfWork.Brands.GetByExternalIdAsync(extId, ct),
            (name, ct) => _unitOfWork.Brands.GetByNameAsync(name, ct),
            cancellationToken);

        Brand brand;

        if (lookup.IsNew)
        {
            // Validate ExternalId is available
            if (string.IsNullOrEmpty(lookup.EffectiveExternalId))
            {
                return Result<BrandDto>.Failure(
                    "ExternalId or Id is required for creating new brands via sync",
                    "EXTERNAL_ID_REQUIRED");
            }

            brand = Brand.CreateFromExternal(
                externalId: lookup.EffectiveExternalId,
                name: request.Name,
                description: request.Description ?? string.Empty,
                logoUrl: request.LogoUrl,
                websiteUrl: request.WebsiteUrl,
                isActive: request.IsActive,
                externalCode: request.ExternalCode,
                specificId: lookup.CreateWithSpecificId ? request.Id : null);

            await _unitOfWork.Brands.AddAsync(brand, cancellationToken);

            _logger.LogInformation(
                "Creating brand from external sync: {ExternalId} - {Name} (Id: {Id})",
                lookup.EffectiveExternalId, request.Name, brand.Id);
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
