using B2BCommerce.Backend.Application.Features.Products.Commands.UpsertProduct;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Products API endpoints for external integrations (LOGO ERP).
/// All operations use ExternalId as the primary identifier.
/// </summary>
public class ProductsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(
        IMediator mediator,
        ILogger<ProductsController> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all products with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.PagedApiResponse<ProductListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        // Resolve CategoryId (ExternalId) to internal Guid if provided
        Guid? categoryId = null;
        if (!string.IsNullOrEmpty(filter.CategoryId))
        {
            var category = await _unitOfWork.Categories.GetByExternalIdAsync(filter.CategoryId);
            if (category == null)
            {
                return NotFoundResponse($"Category with ID '{filter.CategoryId}' not found");
            }
            categoryId = category.Id;
        }

        // Resolve BrandId (ExternalId) to internal Guid if provided
        Guid? brandId = null;
        if (!string.IsNullOrEmpty(filter.BrandId))
        {
            var brand = await _unitOfWork.Brands.GetByExternalIdAsync(filter.BrandId);
            if (brand == null)
            {
                return NotFoundResponse($"Brand with ID '{filter.BrandId}' not found");
            }
            brandId = brand.Id;
        }

        // Resolve ProductTypeId (ExternalId) to internal Guid if provided
        Guid? productTypeId = null;
        if (!string.IsNullOrEmpty(filter.ProductTypeId))
        {
            var productType = await _unitOfWork.ProductTypes.GetByExternalIdAsync(filter.ProductTypeId);
            if (productType == null)
            {
                return NotFoundResponse($"ProductType with ID '{filter.ProductTypeId}' not found");
            }
            productTypeId = productType.Id;
        }

        // Build query
        var products = await GetProductsWithFilters(
            filter.Search,
            categoryId,
            brandId,
            productTypeId,
            filter.IsActive,
            filter.MinStock,
            filter.MaxStock,
            filter.PageNumber,
            filter.PageSize,
            filter.SortBy,
            filter.SortDirection);

        return PagedResponse(
            products.Items.Select(MapToProductListDto).ToList(),
            products.PageNumber,
            products.PageSize,
            products.TotalCount);
    }

    /// <summary>
    /// Get a product by ID (ExternalId)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(string id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsByExternalIdAsync(id);

        if (product == null)
        {
            return NotFoundResponse($"Product with ID '{id}' not found");
        }

        return OkResponse(MapToProductDto(product));
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySku(string sku)
    {
        var product = await _unitOfWork.Products.GetWithDetailsBySKUAsync(sku);

        if (product == null)
        {
            return NotFoundResponse($"Product with SKU '{sku}' not found");
        }

        return OkResponse(MapToProductDto(product));
    }

    /// <summary>
    /// Upsert product. If product with given ID (ExternalId) or SKU exists, it is updated; otherwise, a new product is created.
    /// Id is required for creating new products.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertProduct([FromBody] ProductSyncRequest request)
    {
        // Validate: Id (ExternalId) is required for new products
        if (string.IsNullOrEmpty(request.Id) && string.IsNullOrEmpty(request.SKU))
        {
            return BadRequestResponse("Id or SKU is required", "ID_REQUIRED");
        }

        var command = new UpsertProductCommand
        {
            ExternalId = request.Id,
            ExternalCode = request.Code,
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            CategoryExtId = request.CategoryId,
            BrandExtId = request.BrandId,
            ProductTypeExtId = request.ProductTypeId,
            ListPrice = request.ListPrice,
            Currency = request.Currency,
            Tier1Price = request.Tier1Price,
            Tier2Price = request.Tier2Price,
            Tier3Price = request.Tier3Price,
            Tier4Price = request.Tier4Price,
            Tier5Price = request.Tier5Price,
            StockQuantity = request.StockQuantity,
            MinimumOrderQuantity = request.MinimumOrderQuantity,
            TaxRate = request.TaxRate,
            Status = request.Status,
            MainImageUrl = request.MainImageUrl,
            ImageUrls = request.ImageUrls,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            MainProductExtId = request.MainProductId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "CATEGORY_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "Category not found", result.ErrorCode),
                "PRODUCT_TYPE_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "ProductType not found", result.ErrorCode),
                "BRAND_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "Brand not found", result.ErrorCode),
                "MAIN_PRODUCT_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "Main product not found", result.ErrorCode),
                "CATEGORY_REQUIRED" => BadRequestResponse(result.ErrorMessage ?? "Category is required", result.ErrorCode),
                "EXTERNAL_ID_REQUIRED" => BadRequestResponse(result.ErrorMessage ?? "Id is required for new products", result.ErrorCode),
                _ => BadRequestResponse(result.ErrorMessage ?? "Failed to sync product", result.ErrorCode)
            };
        }

        _logger.LogInformation(
            "Product {Id}/{SKU} synced by API client {ClientName}",
            request.Id,
            request.SKU,
            GetClientName());

        return OkResponse(MapFromApplicationDto(result.Data!));
    }

    /// <summary>
    /// Delete a product by ID (ExternalId) - soft delete
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var product = await _unitOfWork.Products.GetByExternalIdAsync(id);

        if (product == null)
        {
            return NotFoundResponse($"Product with ID '{id}' not found");
        }

        product.MarkAsDeleted(GetClientName());
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ExternalId} deleted by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse<object?>(null, "Product deleted successfully");
    }

    /// <summary>
    /// Delete a product by SKU (soft delete)
    /// </summary>
    [HttpDelete("sku/{sku}")]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductBySku(string sku)
    {
        var product = await _unitOfWork.Products.GetBySKUAsync(sku);

        if (product == null)
        {
            return NotFoundResponse($"Product with SKU '{sku}' not found");
        }

        product.MarkAsDeleted(GetClientName());
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product with SKU {SKU} deleted by API client {ClientName}",
            sku,
            GetClientName());

        return OkResponse<object?>(null, "Product deleted successfully");
    }

    #region Private Methods

    private async Task<(List<Domain.Entities.Product> Items, int PageNumber, int PageSize, int TotalCount)> GetProductsWithFilters(
        string? search,
        Guid? categoryId,
        Guid? brandId,
        Guid? productTypeId,
        bool? isActive,
        int? minStock,
        int? maxStock,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection)
    {
        // Ensure valid pagination
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get all products and filter in memory for now
        // TODO: Implement proper paging in repository
        var allProducts = await _unitOfWork.Products.SearchAsync(
            search ?? string.Empty,
            categoryId,
            brandId,
            activeOnly: isActive ?? false);

        var query = allProducts.AsQueryable();

        // Apply additional filters
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (productTypeId.HasValue)
        {
            query = query.Where(p => p.ProductTypeId == productTypeId.Value);
        }

        if (minStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity <= maxStock.Value);
        }

        // Apply sorting
        query = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("name", _) => query.OrderBy(p => p.Name),
            ("sku", "desc") => query.OrderByDescending(p => p.SKU),
            ("sku", _) => query.OrderBy(p => p.SKU),
            ("createdat", "desc") => query.OrderByDescending(p => p.CreatedAt),
            ("createdat", _) => query.OrderBy(p => p.CreatedAt),
            ("updatedat", "desc") => query.OrderByDescending(p => p.UpdatedAt),
            ("updatedat", _) => query.OrderBy(p => p.UpdatedAt),
            ("stockquantity", "desc") => query.OrderByDescending(p => p.StockQuantity),
            ("stockquantity", _) => query.OrderBy(p => p.StockQuantity),
            ("listprice", "desc") => query.OrderByDescending(p => p.ListPrice.Amount),
            ("listprice", _) => query.OrderBy(p => p.ListPrice.Amount),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, pageNumber, pageSize, totalCount);
    }

    private static ProductDto MapToProductDto(Domain.Entities.Product product)
    {
        var primaryCategory = product.ProductCategories.FirstOrDefault(pc => pc.IsPrimary);

        return new ProductDto
        {
            Id = product.ExternalId ?? string.Empty,
            Code = product.ExternalCode,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CategoryId = primaryCategory?.Category?.ExternalId,
            CategoryName = primaryCategory?.Category?.Name,
            BrandId = product.Brand?.ExternalId,
            BrandName = product.Brand?.Name,
            ProductTypeId = product.ProductType?.ExternalId,
            ProductTypeName = product.ProductType?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            Tier1Price = product.Tier1Price?.Amount,
            Tier2Price = product.Tier2Price?.Amount,
            Tier3Price = product.Tier3Price?.Amount,
            Tier4Price = product.Tier4Price?.Amount,
            Tier5Price = product.Tier5Price?.Amount,
            StockQuantity = product.StockQuantity,
            MinimumOrderQuantity = product.MinimumOrderQuantity,
            TaxRate = product.TaxRate,
            Status = product.Status,
            IsActive = product.IsActive,
            IsSerialTracked = product.IsSerialTracked,
            MainImageUrl = product.MainImageUrl,
            ImageUrls = product.ImageUrls.ToList(),
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            LastSyncedAt = product.LastSyncedAt,
            MainProductId = product.MainProduct?.ExternalId,
            IsVariant = product.IsVariant,
            IsMainProduct = product.IsMainProduct,
            VariantCount = product.Variants?.Count ?? 0,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static ProductListDto MapToProductListDto(Domain.Entities.Product product)
    {
        var primaryCategory = product.ProductCategories.FirstOrDefault(pc => pc.IsPrimary);

        return new ProductListDto
        {
            Id = product.ExternalId ?? string.Empty,
            Code = product.ExternalCode,
            SKU = product.SKU,
            Name = product.Name,
            CategoryId = primaryCategory?.Category?.ExternalId,
            CategoryName = primaryCategory?.Category?.Name,
            BrandId = product.Brand?.ExternalId,
            BrandName = product.Brand?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            StockQuantity = product.StockQuantity,
            Status = product.Status,
            IsActive = product.IsActive,
            MainImageUrl = product.MainImageUrl,
            LastSyncedAt = product.LastSyncedAt,
            MainProductId = product.MainProduct?.ExternalId,
            IsVariant = product.IsVariant,
            VariantCount = product.Variants?.Count ?? 0
        };
    }

    private static ProductDto MapFromApplicationDto(Application.DTOs.Products.ProductDto source)
    {
        return new ProductDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.ExternalCode,
            SKU = source.SKU,
            Name = source.Name,
            Description = source.Description,
            CategoryId = source.CategoryExtId,
            CategoryName = source.CategoryName,
            BrandId = source.BrandExtId,
            BrandName = source.BrandName,
            ProductTypeId = source.ProductTypeExtId,
            ProductTypeName = source.ProductTypeName,
            ListPrice = source.ListPrice,
            Currency = source.Currency,
            Tier1Price = source.Tier1Price,
            Tier2Price = source.Tier2Price,
            Tier3Price = source.Tier3Price,
            Tier4Price = source.Tier4Price,
            Tier5Price = source.Tier5Price,
            StockQuantity = source.StockQuantity,
            MinimumOrderQuantity = source.MinimumOrderQuantity,
            TaxRate = source.TaxRate,
            Status = source.Status,
            IsActive = source.IsActive,
            IsSerialTracked = source.IsSerialTracked,
            MainImageUrl = source.MainImageUrl,
            ImageUrls = source.ImageUrls.ToList(),
            Weight = source.Weight,
            Length = source.Length,
            Width = source.Width,
            Height = source.Height,
            LastSyncedAt = source.LastSyncedAt,
            MainProductId = source.MainProductExtId,
            IsVariant = source.IsVariant,
            IsMainProduct = source.IsMainProduct,
            VariantCount = source.VariantCount,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    #endregion
}
