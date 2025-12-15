using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.UpsertProduct;

/// <summary>
/// Handler for UpsertProductCommand.
/// Creates or updates a product based on ExternalId (primary), Id (fallback), or SKU (fallback).
/// </summary>
public class UpsertProductCommandHandler : ICommandHandler<UpsertProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpsertProductCommandHandler> _logger;

    public UpsertProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpsertProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpsertProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve Category (by ID or ExternalId)
        Guid? categoryId = request.CategoryId;

        if (!categoryId.HasValue && !string.IsNullOrEmpty(request.CategoryExtId))
        {
            var category = await _unitOfWork.Categories
                .GetByExternalIdAsync(request.CategoryExtId, cancellationToken);

            if (category == null)
            {
                return Result<ProductDto>.Failure(
                    $"Category not found by ExternalId: {request.CategoryExtId}",
                    "CATEGORY_NOT_FOUND");
            }

            categoryId = category.Id;
        }

        if (!categoryId.HasValue)
        {
            return Result<ProductDto>.Failure(
                "Category is required. Provide either CategoryId or CategoryExtId.",
                "CATEGORY_REQUIRED");
        }

        // Verify category exists if provided by ID
        if (request.CategoryId.HasValue)
        {
            var categoryEntity = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (categoryEntity == null)
            {
                return Result<ProductDto>.Failure(
                    $"Category not found by Id: {request.CategoryId}",
                    "CATEGORY_NOT_FOUND");
            }
        }

        // 2. Resolve ProductType (by ID or ExternalId) - optional
        Guid? productTypeId = request.ProductTypeId;

        if (!productTypeId.HasValue && !string.IsNullOrEmpty(request.ProductTypeExtId))
        {
            var productType = await _unitOfWork.ProductTypes
                .GetByExternalIdAsync(request.ProductTypeExtId, cancellationToken);

            if (productType == null)
            {
                return Result<ProductDto>.Failure(
                    $"ProductType not found by ExternalId: {request.ProductTypeExtId}",
                    "PRODUCT_TYPE_NOT_FOUND");
            }

            productTypeId = productType.Id;
        }

        // Verify product type exists if provided by ID
        if (request.ProductTypeId.HasValue)
        {
            var productTypeEntity = await _unitOfWork.ProductTypes.GetByIdAsync(request.ProductTypeId.Value, cancellationToken);
            if (productTypeEntity == null)
            {
                return Result<ProductDto>.Failure(
                    $"ProductType not found by Id: {request.ProductTypeId}",
                    "PRODUCT_TYPE_NOT_FOUND");
            }
        }

        // 3. Verify brand exists if provided (by ID or ExternalId)
        Guid? brandId = request.BrandId;
        if (!brandId.HasValue && !string.IsNullOrEmpty(request.BrandExtId))
        {
            var brandEntity = await _unitOfWork.Brands.GetByExternalIdAsync(request.BrandExtId, cancellationToken);
            if (brandEntity == null)
            {
                return Result<ProductDto>.Failure(
                    $"Brand not found by ExternalId: {request.BrandExtId}",
                    "BRAND_NOT_FOUND");
            }
            brandId = brandEntity.Id;
        }
        else if (brandId.HasValue)
        {
            var brandEntity = await _unitOfWork.Brands.GetByIdAsync(brandId.Value, cancellationToken);
            if (brandEntity == null)
            {
                return Result<ProductDto>.Failure(
                    $"Brand not found by Id: {request.BrandId}",
                    "BRAND_NOT_FOUND");
            }
        }

        // 4. Find existing product (by Id, ExternalId, or SKU)
        Product? product = null;
        bool createWithSpecificId = false;

        if (request.Id.HasValue)
        {
            product = await _unitOfWork.Products.GetByIdAsync(request.Id.Value, cancellationToken);

            // If Id is provided but not found, we'll create with that specific Id
            if (product == null)
            {
                createWithSpecificId = true;
            }
        }

        // If not found by Id, try ExternalId (PRIMARY lookup)
        if (product == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalId))
        {
            product = await _unitOfWork.Products
                .GetByExternalIdAsync(request.ExternalId, cancellationToken);
        }

        // If still not found, try SKU as fallback
        if (product == null && !createWithSpecificId)
        {
            product = await _unitOfWork.Products
                .GetBySKUAsync(request.SKU, cancellationToken);
        }

        // 5. Create or update
        if (product == null)
        {
            // Create new product - ExternalId is required for new external entities
            if (string.IsNullOrEmpty(request.ExternalId))
            {
                return Result<ProductDto>.Failure(
                    "ExternalId is required for creating new products via sync",
                    "EXTERNAL_ID_REQUIRED");
            }

            var listPrice = new Money(request.ListPrice, request.Currency);

            product = Product.CreateFromExternal(
                externalId: request.ExternalId,
                sku: request.SKU,
                name: request.Name,
                description: request.Description ?? string.Empty,
                categoryId: categoryId.Value,
                listPrice: listPrice,
                stockQuantity: request.StockQuantity,
                minimumOrderQuantity: request.MinimumOrderQuantity,
                taxRate: request.TaxRate,
                brandId: brandId,
                productTypeId: productTypeId,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            // Set images
            if (!string.IsNullOrEmpty(request.MainImageUrl))
            {
                product.SetMainImage(request.MainImageUrl);
            }

            if (request.ImageUrls != null)
            {
                foreach (var imageUrl in request.ImageUrls)
                {
                    product.AddImage(imageUrl);
                }
            }

            // Set dimensions
            product.UpdateDimensions(request.Weight, request.Length, request.Width, request.Height);

            // Set tier prices
            UpdateTierPrices(product, request);

            product.CreatedBy = request.ModifiedBy;

            await _unitOfWork.Products.AddAsync(product, cancellationToken);

            _logger.LogInformation(
                "Creating product from external sync: {ExternalId} - {SKU} - {Name} (Id: {Id})",
                request.ExternalId, request.SKU, request.Name, product.Id);
        }
        else
        {
            // Update existing product
            product.UpdateFromExternal(
                name: request.Name,
                description: request.Description ?? string.Empty,
                categoryId: categoryId.Value,
                brandId: brandId,
                productTypeId: productTypeId,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            // Update pricing
            var listPrice = new Money(request.ListPrice, request.Currency);
            UpdateTierPrices(product, request, listPrice);

            // Update stock
            product.UpdateStock(request.StockQuantity);

            // Update images
            if (!string.IsNullOrEmpty(request.MainImageUrl))
            {
                product.SetMainImage(request.MainImageUrl);
            }

            // Update dimensions
            product.UpdateDimensions(request.Weight, request.Length, request.Width, request.Height);

            product.UpdatedBy = request.ModifiedBy;
            product.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Updating product from external sync: {ExternalId} - {SKU} - {Name}",
                product.ExternalId, product.SKU, request.Name);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Get related data for response
        var productWithDetails = await _unitOfWork.Products
            .GetWithDetailsByIdAsync(product.Id, cancellationToken);

        // 7. Map to DTO and return
        var dto = MapToDto(productWithDetails!);

        return Result<ProductDto>.Success(dto);
    }

    private static void UpdateTierPrices(Product product, UpsertProductCommand request, Money? listPrice = null)
    {
        var currency = request.Currency;
        var list = listPrice ?? new Money(request.ListPrice, currency);
        var tier1 = request.Tier1Price.HasValue ? new Money(request.Tier1Price.Value, currency) : null;
        var tier2 = request.Tier2Price.HasValue ? new Money(request.Tier2Price.Value, currency) : null;
        var tier3 = request.Tier3Price.HasValue ? new Money(request.Tier3Price.Value, currency) : null;
        var tier4 = request.Tier4Price.HasValue ? new Money(request.Tier4Price.Value, currency) : null;
        var tier5 = request.Tier5Price.HasValue ? new Money(request.Tier5Price.Value, currency) : null;

        product.UpdatePricing(list, tier1, tier2, tier3, tier4, tier5);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            Tier1Price = product.Tier1Price?.Amount,
            Tier2Price = product.Tier2Price?.Amount,
            Tier3Price = product.Tier3Price?.Amount,
            Tier4Price = product.Tier4Price?.Amount,
            Tier5Price = product.Tier5Price?.Amount,
            StockQuantity = product.StockQuantity,
            MinimumOrderQuantity = product.MinimumOrderQuantity,
            IsActive = product.IsActive,
            IsSerialTracked = product.IsSerialTracked,
            TaxRate = product.TaxRate,
            MainImageUrl = product.MainImageUrl,
            ImageUrls = product.ImageUrls.ToList(),
            Specifications = new Dictionary<string, string>(product.Specifications),
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt ?? DateTime.MinValue,
            ExternalId = product.ExternalId,
            ExternalCode = product.ExternalCode,
            LastSyncedAt = product.LastSyncedAt,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType?.Name,
            ProductTypeExtId = product.ProductType?.ExternalId,
            CategoryExtId = product.Category?.ExternalId
        };
    }
}
