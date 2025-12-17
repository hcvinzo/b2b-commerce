using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Product service implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        return Result<ProductDto>.Success(await MapToProductDtoAsync(product, cancellationToken));
    }

    public async Task<Result<ProductDto>> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetBySKUAsync(sku, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        return Result<ProductDto>.Success(await MapToProductDtoAsync(product, cancellationToken));
    }

    public async Task<Result<PagedResult<ProductListDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var totalCount = products.Count();

        var pagedProducts = products
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToProductListDto)
            .ToList();

        var pagedResult = new PagedResult<ProductListDto>(pagedProducts, pageNumber, pageSize, totalCount);
        return Result<PagedResult<ProductListDto>>.Success(pagedResult);
    }

    public async Task<Result<IEnumerable<ProductListDto>>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId, false, cancellationToken);
        var productDtos = products.Select(MapToProductListDto);
        return Result<IEnumerable<ProductListDto>>.Success(productDtos);
    }

    public async Task<Result<PagedResult<ProductListDto>>> SearchAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.SearchAsync(searchTerm, null, null, true, cancellationToken);
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToProductListDto)
            .ToList();

        var pagedResult = new PagedResult<ProductListDto>(pagedProducts, pageNumber, pageSize, totalCount);
        return Result<PagedResult<ProductListDto>>.Success(pagedResult);
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        // Check if SKU already exists
        var existingProduct = await _unitOfWork.Products.GetBySKUAsync(dto.SKU, cancellationToken);
        if (existingProduct is not null)
        {
            return Result<ProductDto>.Failure("Product with this SKU already exists", "SKU_EXISTS");
        }

        // Validate at least one category
        if (dto.CategoryIds is null || dto.CategoryIds.Count == 0)
        {
            return Result<ProductDto>.Failure("At least one category is required", "CATEGORY_REQUIRED");
        }

        // Get primary category (first one in the list)
        var primaryCategoryId = dto.CategoryIds[0];

        try
        {
            var product = Product.Create(
                name: dto.Name,
                description: dto.Description,
                sku: dto.SKU,
                listPrice: new Money(dto.ListPrice, dto.Currency),
                stockQuantity: dto.StockQuantity,
                minimumOrderQuantity: dto.MinimumOrderQuantity,
                taxRate: dto.TaxRate
            );

            // Set brand
            if (dto.BrandId.HasValue)
            {
                product.UpdateBasicInfo(dto.Name, dto.Description, dto.BrandId);
            }

            // Add product to categories (first one is primary)
            for (int i = 0; i < dto.CategoryIds.Count; i++)
            {
                var categoryId = dto.CategoryIds[i];
                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
                if (category is null)
                {
                    return Result<ProductDto>.Failure($"Category not found: {categoryId}", "CATEGORY_NOT_FOUND");
                }
                product.AddToCategory(categoryId, isPrimary: i == 0, displayOrder: i);
            }

            // Set tier prices
            product.UpdatePricing(
                listPrice: new Money(dto.ListPrice, dto.Currency),
                tier1Price: dto.Tier1Price.HasValue ? new Money(dto.Tier1Price.Value, dto.Currency) : null,
                tier2Price: dto.Tier2Price.HasValue ? new Money(dto.Tier2Price.Value, dto.Currency) : null,
                tier3Price: dto.Tier3Price.HasValue ? new Money(dto.Tier3Price.Value, dto.Currency) : null,
                tier4Price: dto.Tier4Price.HasValue ? new Money(dto.Tier4Price.Value, dto.Currency) : null,
                tier5Price: dto.Tier5Price.HasValue ? new Money(dto.Tier5Price.Value, dto.Currency) : null
            );

            // Set images
            if (!string.IsNullOrEmpty(dto.MainImageUrl))
            {
                product.SetMainImage(dto.MainImageUrl);
            }

            if (dto.ImageUrls is not null)
            {
                foreach (var imageUrl in dto.ImageUrls)
                {
                    product.AddImage(imageUrl);
                }
            }

            // Set specifications
            if (dto.Specifications is not null)
            {
                foreach (var spec in dto.Specifications)
                {
                    product.AddSpecification(spec.Key, spec.Value);
                }
            }

            // Set dimensions
            product.UpdateDimensions(dto.Weight, dto.Length, dto.Width, dto.Height);

            // Set main product (variant relationship)
            if (dto.MainProductId.HasValue)
            {
                var mainProduct = await _unitOfWork.Products.GetByIdAsync(dto.MainProductId.Value, cancellationToken);
                if (mainProduct is null)
                {
                    return Result<ProductDto>.Failure("Main product not found", "MAIN_PRODUCT_NOT_FOUND");
                }
                product.SetMainProduct(dto.MainProductId.Value);
            }

            // Set product type
            if (dto.ProductTypeId.HasValue)
            {
                var productType = await _unitOfWork.ProductTypes.GetByIdAsync(dto.ProductTypeId.Value, cancellationToken);
                if (productType is null)
                {
                    return Result<ProductDto>.Failure("Product type not found", "PRODUCT_TYPE_NOT_FOUND");
                }
                product.SetProductType(dto.ProductTypeId.Value);
            }

            // Set attribute values
            if (dto.AttributeValues is not null)
            {
                var setAttributesResult = await SetProductAttributeValuesAsync(product, dto.AttributeValues, cancellationToken);
                if (!setAttributesResult.IsSuccess)
                {
                    return Result<ProductDto>.Failure(setAttributesResult.ErrorMessage ?? "Failed to set attributes", setAttributesResult.ErrorCode ?? "ATTRIBUTE_ERROR");
                }
            }

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product created with ID {ProductId} and SKU {SKU}", product.Id, product.SKU);

            return Result<ProductDto>.Success(await MapToProductDtoAsync(product, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating product");
            return Result<ProductDto>.Failure(ex.Message, "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Result<ProductDto>.Failure("Failed to create product", "CREATE_FAILED");
        }
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        try
        {
            // Handle categories - update ProductCategories
            if (dto.CategoryIds is not null && dto.CategoryIds.Count > 0)
            {
                // Validate all categories exist
                foreach (var categoryId in dto.CategoryIds)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
                    if (category is null)
                    {
                        return Result<ProductDto>.Failure($"Category not found: {categoryId}", "CATEGORY_NOT_FOUND");
                    }
                }

                // Get current category IDs
                var currentCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();

                // Remove categories that are no longer in the list
                foreach (var categoryId in currentCategoryIds)
                {
                    if (!dto.CategoryIds.Contains(categoryId))
                    {
                        product.RemoveFromCategory(categoryId);
                    }
                }

                // Add new categories and update primary
                for (int i = 0; i < dto.CategoryIds.Count; i++)
                {
                    var categoryId = dto.CategoryIds[i];
                    var isPrimary = i == 0;

                    if (!currentCategoryIds.Contains(categoryId))
                    {
                        // New category
                        product.AddToCategory(categoryId, isPrimary: isPrimary, displayOrder: i);
                    }
                    else if (isPrimary)
                    {
                        // Set as primary if it's the first one
                        product.SetPrimaryCategory(categoryId);
                    }
                }
            }

            // Update basic info
            if (!string.IsNullOrEmpty(dto.Name))
            {
                product.UpdateBasicInfo(
                    name: dto.Name,
                    description: dto.Description ?? product.Description,
                    brandId: dto.BrandId ?? product.BrandId
                );
            }

            // Update pricing if list price is provided (non-zero)
            if (dto.ListPrice > 0)
            {
                var currency = !string.IsNullOrEmpty(dto.Currency) ? dto.Currency : product.ListPrice.Currency;
                product.UpdatePricing(
                    listPrice: new Money(dto.ListPrice, currency),
                    tier1Price: dto.Tier1Price.HasValue ? new Money(dto.Tier1Price.Value, currency) : product.Tier1Price,
                    tier2Price: dto.Tier2Price.HasValue ? new Money(dto.Tier2Price.Value, currency) : product.Tier2Price,
                    tier3Price: dto.Tier3Price.HasValue ? new Money(dto.Tier3Price.Value, currency) : product.Tier3Price,
                    tier4Price: dto.Tier4Price.HasValue ? new Money(dto.Tier4Price.Value, currency) : product.Tier4Price,
                    tier5Price: dto.Tier5Price.HasValue ? new Money(dto.Tier5Price.Value, currency) : product.Tier5Price
                );
            }

            // Update stock (always update since it's non-nullable in DTO)
            product.UpdateStock(dto.StockQuantity);

            // Update image if provided
            if (!string.IsNullOrEmpty(dto.MainImageUrl))
            {
                product.SetMainImage(dto.MainImageUrl);
            }

            // Update dimensions if provided
            if (dto.Weight.HasValue || dto.Length.HasValue || dto.Width.HasValue || dto.Height.HasValue)
            {
                product.UpdateDimensions(
                    dto.Weight ?? product.Weight,
                    dto.Length ?? product.Length,
                    dto.Width ?? product.Width,
                    dto.Height ?? product.Height
                );
            }

            // Update status if provided
            if (dto.Status.HasValue)
            {
                product.SetStatus(dto.Status.Value);
            }

            // Update main product (variant relationship)
            if (dto.ClearMainProduct)
            {
                product.ClearMainProduct();
            }
            else if (dto.MainProductId.HasValue)
            {
                var mainProduct = await _unitOfWork.Products.GetByIdAsync(dto.MainProductId.Value, cancellationToken);
                if (mainProduct is null)
                {
                    return Result<ProductDto>.Failure("Main product not found", "MAIN_PRODUCT_NOT_FOUND");
                }
                product.SetMainProduct(dto.MainProductId.Value);
            }

            // Update product type
            if (dto.ClearProductType)
            {
                product.ClearProductType();
            }
            else if (dto.ProductTypeId.HasValue)
            {
                var productType = await _unitOfWork.ProductTypes.GetByIdAsync(dto.ProductTypeId.Value, cancellationToken);
                if (productType is null)
                {
                    return Result<ProductDto>.Failure("Product type not found", "PRODUCT_TYPE_NOT_FOUND");
                }
                product.SetProductType(dto.ProductTypeId.Value);
            }

            // Update attribute values
            if (dto.AttributeValues is not null)
            {
                // Clear existing attribute values and set new ones
                product.ClearAttributeValues();
                var setAttributesResult = await SetProductAttributeValuesAsync(product, dto.AttributeValues, cancellationToken);
                if (!setAttributesResult.IsSuccess)
                {
                    return Result<ProductDto>.Failure(setAttributesResult.ErrorMessage ?? "Failed to set attributes", setAttributesResult.ErrorCode ?? "ATTRIBUTE_ERROR");
                }
            }

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product updated with ID {ProductId}", product.Id);

            return Result<ProductDto>.Success(await MapToProductDtoAsync(product, cancellationToken));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation error updating product");
            return Result<ProductDto>.Failure(ex.Message, "DOMAIN_VALIDATION_ERROR");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating product");
            return Result<ProductDto>.Failure(ex.Message, "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return Result<ProductDto>.Failure("Failed to update product", "UPDATE_FAILED");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted with ID {ProductId}", id);

        return Result.Success();
    }

    public async Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        try
        {
            product.Activate();
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product activated with ID {ProductId}", id);

            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            _logger.LogWarning(ex, "Cannot activate product {ProductId}: {Message}", id, ex.Message);
            return Result.Failure(ex.Message, "ACTIVATION_FAILED");
        }
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        product.Deactivate();
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deactivated with ID {ProductId}", id);

        return Result.Success();
    }

    private ProductDto MapToProductDto(Product product, Product? mainProduct = null, int variantCount = 0)
    {
        // Map product categories
        var categories = product.ProductCategories
            .OrderBy(pc => pc.IsPrimary ? 0 : 1)
            .ThenBy(pc => pc.DisplayOrder)
            .Select(pc => new ProductCategoryDto
            {
                Id = pc.Id,
                CategoryId = pc.CategoryId,
                CategoryName = pc.Category?.Name ?? "",
                CategorySlug = pc.Category?.Slug ?? "",
                IsPrimary = pc.IsPrimary,
                DisplayOrder = pc.DisplayOrder
            })
            .ToList();

        // Get primary category info
        var primaryCategory = product.ProductCategories.FirstOrDefault(pc => pc.IsPrimary);
        var categoryId = primaryCategory?.CategoryId;
        var categoryName = primaryCategory?.Category?.Name;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            CategoryId = categoryId,
            CategoryName = categoryName,
            Categories = categories,
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
            Status = product.Status,
            IsActive = product.IsActive,
            IsSerialTracked = product.IsSerialTracked,
            TaxRate = product.TaxRate,
            MainImageUrl = product.MainImageUrl,
            ImageUrls = product.ImageUrls ?? new List<string>(),
            Specifications = product.Specifications ?? new Dictionary<string, string>(),
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType?.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt ?? product.CreatedAt,
            MainProductId = product.MainProductId,
            MainProductSku = mainProduct?.SKU,
            MainProductExtId = mainProduct?.ExternalId,
            IsVariant = product.IsVariant,
            IsMainProduct = product.IsMainProduct,
            VariantCount = variantCount,
            AttributeValues = MapAttributeValues(product)
        };
    }

    private async Task<ProductDto> MapToProductDtoAsync(Product product, CancellationToken cancellationToken)
    {
        Product? mainProduct = null;
        int variantCount = 0;

        if (product.MainProductId.HasValue)
        {
            mainProduct = await _unitOfWork.Products.GetByIdAsync(product.MainProductId.Value, cancellationToken);
        }

        if (product.IsMainProduct)
        {
            variantCount = await _unitOfWork.Products.GetVariantCountAsync(product.Id, cancellationToken);
        }

        return MapToProductDto(product, mainProduct, variantCount);
    }

    private static ProductListDto MapToProductListDto(Product product)
    {
        var primaryCategory = product.ProductCategories.FirstOrDefault(pc => pc.IsPrimary);

        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            CategoryName = primaryCategory?.Category?.Name,
            BrandName = product.Brand?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            StockQuantity = product.StockQuantity,
            Status = product.Status,
            IsActive = product.IsActive,
            MainImageUrl = product.MainImageUrl,
            MainProductId = product.MainProductId,
            IsVariant = product.IsVariant,
            VariantCount = product.Variants?.Count ?? 0
        };
    }

    private async Task<Result> SetProductAttributeValuesAsync(
        Product product,
        List<ProductAttributeValueInputDto> attributeValues,
        CancellationToken cancellationToken)
    {
        foreach (var attrValue in attributeValues)
        {
            var attributeDef = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attrValue.AttributeDefinitionId, cancellationToken);
            if (attributeDef is null)
            {
                return Result.Failure($"Attribute definition not found: {attrValue.AttributeDefinitionId}", "ATTRIBUTE_NOT_FOUND");
            }

            // Set value based on attribute type
            switch (attributeDef.Type)
            {
                case Domain.Enums.AttributeType.Text:
                    if (attrValue.TextValue is not null)
                    {
                        product.SetTextAttribute(attrValue.AttributeDefinitionId, attrValue.TextValue);
                    }
                    break;

                case Domain.Enums.AttributeType.Number:
                    if (attrValue.NumericValue.HasValue)
                    {
                        product.SetNumericAttribute(attrValue.AttributeDefinitionId, attrValue.NumericValue.Value);
                    }
                    break;

                case Domain.Enums.AttributeType.Select:
                    if (attrValue.SelectValueId.HasValue)
                    {
                        product.SetSelectAttribute(attrValue.AttributeDefinitionId, attrValue.SelectValueId.Value);
                    }
                    break;

                case Domain.Enums.AttributeType.MultiSelect:
                    if (attrValue.MultiSelectValueIds is not null && attrValue.MultiSelectValueIds.Count > 0)
                    {
                        product.SetMultiSelectAttribute(attrValue.AttributeDefinitionId, attrValue.MultiSelectValueIds);
                    }
                    break;

                case Domain.Enums.AttributeType.Boolean:
                    if (attrValue.BooleanValue.HasValue)
                    {
                        product.SetBooleanAttribute(attrValue.AttributeDefinitionId, attrValue.BooleanValue.Value);
                    }
                    break;

                case Domain.Enums.AttributeType.Date:
                    if (attrValue.DateValue.HasValue)
                    {
                        product.SetDateAttribute(attrValue.AttributeDefinitionId, attrValue.DateValue.Value);
                    }
                    break;
            }
        }

        return Result.Success();
    }

    private List<ProductAttributeValueOutputDto> MapAttributeValues(Product product)
    {
        var result = new List<ProductAttributeValueOutputDto>();

        foreach (var attrValue in product.AttributeValues)
        {
            var attributeDef = attrValue.AttributeDefinition;
            if (attributeDef is null)
            {
                continue;
            }

            var dto = new ProductAttributeValueOutputDto
            {
                AttributeDefinitionId = attrValue.AttributeDefinitionId,
                AttributeCode = attributeDef.Code,
                AttributeName = attributeDef.Name,
                AttributeType = attributeDef.Type,
                Unit = attributeDef.Unit,
                TextValue = attrValue.TextValue,
                NumericValue = attrValue.NumericValue,
                SelectValueId = attrValue.AttributeValueId,
                BooleanValue = attrValue.BooleanValue,
                DateValue = attrValue.DateValue
            };

            // Get display text for select value
            if (attrValue.AttributeValueId.HasValue && attributeDef.PredefinedValues is not null)
            {
                var selectedValue = attributeDef.PredefinedValues.FirstOrDefault(v => v.Id == attrValue.AttributeValueId);
                dto.SelectValueText = selectedValue?.DisplayText ?? selectedValue?.Value;
            }

            // Get display texts for multi-select values
            var multiSelectIds = attrValue.GetMultiSelectValueIds();
            if (multiSelectIds.Count > 0 && attributeDef.PredefinedValues is not null)
            {
                dto.MultiSelectValueIds = multiSelectIds;
                dto.MultiSelectValueTexts = attributeDef.PredefinedValues
                    .Where(v => multiSelectIds.Contains(v.Id))
                    .Select(v => v.DisplayText ?? v.Value)
                    .ToList();
            }

            result.Add(dto);
        }

        return result;
    }
}
