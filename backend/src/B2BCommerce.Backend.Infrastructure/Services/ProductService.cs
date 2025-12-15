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

        return Result<ProductDto>.Success(MapToProductDto(product));
    }

    public async Task<Result<ProductDto>> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetBySKUAsync(sku, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        return Result<ProductDto>.Success(MapToProductDto(product));
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

        try
        {
            var product = new Product(
                name: dto.Name,
                description: dto.Description,
                sku: dto.SKU,
                categoryId: dto.CategoryId,
                listPrice: new Money(dto.ListPrice, dto.Currency),
                stockQuantity: dto.StockQuantity,
                minimumOrderQuantity: dto.MinimumOrderQuantity,
                taxRate: dto.TaxRate
            );

            // Set brand
            if (dto.BrandId.HasValue)
            {
                product.UpdateBasicInfo(dto.Name, dto.Description, dto.CategoryId, dto.BrandId);
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

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product created with ID {ProductId} and SKU {SKU}", product.Id, product.SKU);

            return Result<ProductDto>.Success(MapToProductDto(product));
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
            // Update basic info
            if (!string.IsNullOrEmpty(dto.Name))
            {
                product.UpdateBasicInfo(
                    name: dto.Name,
                    description: dto.Description ?? product.Description,
                    categoryId: dto.CategoryId != Guid.Empty ? dto.CategoryId : product.CategoryId,
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

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product updated with ID {ProductId}", product.Id);

            return Result<ProductDto>.Success(MapToProductDto(product));
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

        product.Activate();
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product activated with ID {ProductId}", id);

        return Result.Success();
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

    private static ProductDto MapToProductDto(Product product)
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
            ImageUrls = product.ImageUrls ?? new List<string>(),
            Specifications = product.Specifications ?? new Dictionary<string, string>(),
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt ?? product.CreatedAt
        };
    }

    private static ProductListDto MapToProductListDto(Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            CategoryName = product.Category?.Name,
            BrandName = product.Brand?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            MainImageUrl = product.MainImageUrl
        };
    }
}
