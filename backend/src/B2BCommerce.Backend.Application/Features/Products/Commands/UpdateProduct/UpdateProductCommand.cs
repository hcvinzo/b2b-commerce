using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    List<Guid> CategoryIds,
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
    ProductStatus? Status,
    bool IsSerialTracked,
    decimal TaxRate,
    string? MainImageUrl,
    List<string>? ImageUrls,
    Dictionary<string, string>? Specifications,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    Guid? MainProductId,
    bool ClearMainProduct,
    Guid? ProductTypeId,
    bool ClearProductType,
    List<ProductAttributeValueInputDto>? AttributeValues) : ICommand<Result<ProductDto>>;
