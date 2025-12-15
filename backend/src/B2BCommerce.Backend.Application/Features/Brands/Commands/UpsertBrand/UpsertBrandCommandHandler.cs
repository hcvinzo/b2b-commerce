using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
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
        // Find existing brand (by Id, ExternalId, or Name)
        Brand? brand = null;
        bool createWithSpecificId = false;

        if (request.Id.HasValue)
        {
            brand = await _unitOfWork.Brands.GetByIdAsync(request.Id.Value, cancellationToken);

            // If Id is provided but not found, we'll create with that specific Id
            if (brand == null)
            {
                createWithSpecificId = true;
            }
        }

        // If not found by Id, try ExternalId (PRIMARY lookup)
        if (brand == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalId))
        {
            brand = await _unitOfWork.Brands
                .GetByExternalIdAsync(request.ExternalId, cancellationToken);
        }

        // If still not found, try Name as fallback
        if (brand == null && !createWithSpecificId)
        {
            brand = await _unitOfWork.Brands
                .GetByNameAsync(request.Name, cancellationToken);
        }

        // Create or update
        if (brand == null)
        {
            // Create new brand - ExternalId is required for new external entities
            if (string.IsNullOrEmpty(request.ExternalId))
            {
                return Result<BrandDto>.Failure(
                    "ExternalId is required for creating new brands via sync",
                    "EXTERNAL_ID_REQUIRED");
            }

            brand = Brand.CreateFromExternal(
                externalId: request.ExternalId,
                name: request.Name,
                description: request.Description ?? string.Empty,
                logoUrl: request.LogoUrl,
                websiteUrl: request.WebsiteUrl,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            brand.CreatedBy = request.ModifiedBy;

            await _unitOfWork.Brands.AddAsync(brand, cancellationToken);

            _logger.LogInformation(
                "Creating brand from external sync: {ExternalId} - {Name} (Id: {Id})",
                request.ExternalId, request.Name, brand.Id);
        }
        else
        {
            // Update existing brand
            brand.UpdateFromExternal(
                name: request.Name,
                description: request.Description ?? string.Empty,
                logoUrl: request.LogoUrl,
                websiteUrl: request.WebsiteUrl,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            brand.UpdatedBy = request.ModifiedBy;
            brand.UpdatedAt = DateTime.UtcNow;

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
