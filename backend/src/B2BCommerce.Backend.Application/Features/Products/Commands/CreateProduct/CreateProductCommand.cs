using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    Guid CategoryId,
    Guid? BrandId,
    decimal ListPrice,
    string Currency,
    decimal? Tier1Price,
    decimal? Tier2Price,
    decimal? Tier3Price,
    decimal? Tier4Price,
    decimal? Tier5Price,
    int StockQuantity,
    int MinimumOrderQuantity,
    bool IsSerialTracked,
    decimal TaxRate,
    string? MainImageUrl,
    List<string>? ImageUrls,
    Dictionary<string, string>? Specifications,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    Guid? MainProductId) : ICommand<Result<ProductDto>>;
