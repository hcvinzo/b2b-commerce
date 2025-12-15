using B2BCommerce.Backend.Application.Features.Products.Commands.UpsertProduct;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Products API endpoints for external integrations (LOGO ERP)
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
        // Resolve CategoryExtId to CategoryId if provided
        Guid? categoryId = filter.CategoryId;
        if (!categoryId.HasValue && !string.IsNullOrEmpty(filter.CategoryExtId))
        {
            var category = await _unitOfWork.Categories.GetByExternalIdAsync(filter.CategoryExtId);
            if (category == null)
            {
                return NotFoundResponse($"Category with external ID '{filter.CategoryExtId}' not found");
            }
            categoryId = category.Id;
        }

        // Resolve ProductTypeExtId to ProductTypeId if provided
        Guid? productTypeId = filter.ProductTypeId;
        if (!productTypeId.HasValue && !string.IsNullOrEmpty(filter.ProductTypeExtId))
        {
            var productType = await _unitOfWork.ProductTypes.GetByExternalIdAsync(filter.ProductTypeExtId);
            if (productType == null)
            {
                return NotFoundResponse($"ProductType with external ID '{filter.ProductTypeExtId}' not found");
            }
            productTypeId = productType.Id;
        }

        // Build query
        var products = await GetProductsWithFilters(
            filter.Search,
            categoryId,
            filter.BrandId,
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
    /// Get a product by internal ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsByIdAsync(id);

        if (product == null)
        {
            return NotFoundResponse($"Product with ID '{id}' not found");
        }

        return OkResponse(MapToProductDto(product));
    }

    /// <summary>
    /// Get product by external ID (PRIMARY lookup)
    /// </summary>
    [HttpGet("ext/{extId}")]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductByExtId(string extId)
    {
        var product = await _unitOfWork.Products.GetWithDetailsByExternalIdAsync(extId);

        if (product == null)
        {
            return NotFoundResponse($"Product with external ID '{extId}' not found");
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
    /// Upsert product. If product with given external ID, internal ID, or SKU exists, it is updated; otherwise, a new product is created.
    /// ExtId is required for creating new products.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertProduct([FromBody] ProductSyncRequest request)
    {
        // Validate: At least one identifier should be provided for matching
        // For new products, ExtId is required

        var command = new UpsertProductCommand
        {
            Id = request.Id,
            ExternalId = request.ExtId,
            ExternalCode = request.ExtCode,
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            CategoryExtId = request.CategoryExtId,
            BrandId = request.BrandId,
            ProductTypeId = request.ProductTypeId,
            ProductTypeExtId = request.ProductTypeExtId,
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
            IsActive = request.IsActive,
            MainImageUrl = request.MainImageUrl,
            ImageUrls = request.ImageUrls,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            ModifiedBy = GetClientName()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "CATEGORY_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "Category not found", result.ErrorCode),
                "PRODUCT_TYPE_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "ProductType not found", result.ErrorCode),
                "BRAND_NOT_FOUND" => BadRequestResponse(result.ErrorMessage ?? "Brand not found", result.ErrorCode),
                "CATEGORY_REQUIRED" => BadRequestResponse(result.ErrorMessage ?? "Category is required", result.ErrorCode),
                "EXTERNAL_ID_REQUIRED" => BadRequestResponse(result.ErrorMessage ?? "ExternalId is required for new products", result.ErrorCode),
                _ => BadRequestResponse(result.ErrorMessage ?? "Failed to sync product", result.ErrorCode)
            };
        }

        _logger.LogInformation(
            "Product {ExtId}/{SKU} synced by API client {ClientName}",
            request.ExtId,
            request.SKU,
            GetClientName());

        return OkResponse(MapFromApplicationDto(result.Data!));
    }

    /// <summary>
    /// Delete a product by internal ID (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            return NotFoundResponse($"Product with ID '{id}' not found");
        }

        product.MarkAsDeleted(GetClientName());
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} deleted by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse<object?>(null, "Product deleted successfully");
    }

    /// <summary>
    /// Delete a product by external ID (soft delete)
    /// </summary>
    [HttpDelete("ext/{extId}")]
    [Authorize(Policy = "products:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductByExtId(string extId)
    {
        var product = await _unitOfWork.Products.GetByExternalIdAsync(extId);

        if (product == null)
        {
            return NotFoundResponse($"Product with external ID '{extId}' not found");
        }

        product.MarkAsDeleted(GetClientName());
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product with external ID {ExternalId} deleted by API client {ClientName}",
            extId,
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
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CategoryExtId = product.Category?.ExternalId,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType?.Name,
            ProductTypeExtId = product.ProductType?.ExternalId,
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
            IsActive = product.IsActive,
            IsSerialTracked = product.IsSerialTracked,
            MainImageUrl = product.MainImageUrl,
            ImageUrls = product.ImageUrls.ToList(),
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            ExtId = product.ExternalId,
            ExtCode = product.ExternalCode,
            LastSyncedAt = product.LastSyncedAt,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static ProductListDto MapToProductListDto(Domain.Entities.Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            CategoryName = product.Category?.Name,
            BrandName = product.Brand?.Name,
            ListPrice = product.ListPrice.Amount,
            Currency = product.ListPrice.Currency,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            MainImageUrl = product.MainImageUrl,
            ExtId = product.ExternalId,
            ExtCode = product.ExternalCode,
            LastSyncedAt = product.LastSyncedAt
        };
    }

    private static ProductDto MapFromApplicationDto(Application.DTOs.Products.ProductDto source)
    {
        return new ProductDto
        {
            Id = source.Id,
            SKU = source.SKU,
            Name = source.Name,
            Description = source.Description,
            CategoryId = source.CategoryId,
            CategoryName = source.CategoryName,
            CategoryExtId = source.CategoryExtId,
            BrandId = source.BrandId,
            BrandName = source.BrandName,
            ProductTypeId = source.ProductTypeId,
            ProductTypeName = source.ProductTypeName,
            ProductTypeExtId = source.ProductTypeExtId,
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
            IsActive = source.IsActive,
            IsSerialTracked = source.IsSerialTracked,
            MainImageUrl = source.MainImageUrl,
            ImageUrls = source.ImageUrls.ToList(),
            Weight = source.Weight,
            Length = source.Length,
            Width = source.Width,
            Height = source.Height,
            ExtId = source.ExternalId,
            ExtCode = source.ExternalCode,
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    #endregion
}
