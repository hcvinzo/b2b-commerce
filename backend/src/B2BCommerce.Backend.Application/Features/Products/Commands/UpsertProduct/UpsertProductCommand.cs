using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.UpsertProduct;

/// <summary>
/// Command to upsert a product (create or update).
/// Used for external system synchronization (LOGO ERP).
/// Matches by ExternalId (primary), Id (fallback), or SKU (fallback).
/// </summary>
public record UpsertProductCommand : ICommand<Result<ProductDto>>
{
    // Identification (one required for upsert)

    /// <summary>
    /// Internal ID (for internal updates)
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// External system ID (PRIMARY upsert key for LOGO ERP)
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    public string? ExternalCode { get; init; }

    // Required fields

    /// <summary>
    /// Stock Keeping Unit (required, unique, can also be used for matching)
    /// </summary>
    public required string SKU { get; init; }

    /// <summary>
    /// Product name (required)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; init; }

    // Category - support ID or ExternalId

    /// <summary>
    /// Category internal ID
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Category external ID (for external system references)
    /// </summary>
    public string? CategoryExtId { get; init; }

    // Brand - support ID or ExternalId

    /// <summary>
    /// Brand internal ID
    /// </summary>
    public Guid? BrandId { get; init; }

    /// <summary>
    /// Brand external ID (for external system references)
    /// </summary>
    public string? BrandExtId { get; init; }

    // ProductType - support ID or ExternalId

    /// <summary>
    /// Product type internal ID
    /// </summary>
    public Guid? ProductTypeId { get; init; }

    /// <summary>
    /// Product type external ID (for external system references)
    /// </summary>
    public string? ProductTypeExtId { get; init; }

    // Pricing

    /// <summary>
    /// List price amount (required)
    /// </summary>
    public required decimal ListPrice { get; init; }

    /// <summary>
    /// Currency code (default: TRY)
    /// </summary>
    public string Currency { get; init; } = "TRY";

    /// <summary>
    /// Tier 1 price amount
    /// </summary>
    public decimal? Tier1Price { get; init; }

    /// <summary>
    /// Tier 2 price amount
    /// </summary>
    public decimal? Tier2Price { get; init; }

    /// <summary>
    /// Tier 3 price amount
    /// </summary>
    public decimal? Tier3Price { get; init; }

    /// <summary>
    /// Tier 4 price amount
    /// </summary>
    public decimal? Tier4Price { get; init; }

    /// <summary>
    /// Tier 5 price amount
    /// </summary>
    public decimal? Tier5Price { get; init; }

    // Stock

    /// <summary>
    /// Current stock quantity
    /// </summary>
    public int StockQuantity { get; init; } = 0;

    /// <summary>
    /// Minimum order quantity
    /// </summary>
    public int MinimumOrderQuantity { get; init; } = 1;

    // Tax

    /// <summary>
    /// Tax rate (e.g., 0.20 for 20%)
    /// </summary>
    public decimal TaxRate { get; init; } = 0.20m;

    // Status

    /// <summary>
    /// Product status (Draft, Active, Inactive).
    /// If not provided, status is auto-determined based on required fields:
    /// - Active if all required fields (Category, ProductType, ListPrice, TaxRate) are present
    /// - Draft otherwise
    /// </summary>
    public ProductStatus? Status { get; init; }

    // Images

    /// <summary>
    /// Main product image URL
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// Additional image URLs
    /// </summary>
    public List<string>? ImageUrls { get; init; }

    // Dimensions

    /// <summary>
    /// Weight in kilograms
    /// </summary>
    public decimal? Weight { get; init; }

    /// <summary>
    /// Length in centimeters
    /// </summary>
    public decimal? Length { get; init; }

    /// <summary>
    /// Width in centimeters
    /// </summary>
    public decimal? Width { get; init; }

    /// <summary>
    /// Height in centimeters
    /// </summary>
    public decimal? Height { get; init; }

    // Variant support

    /// <summary>
    /// Main product internal ID (for internal references)
    /// </summary>
    public Guid? MainProductId { get; init; }

    /// <summary>
    /// Main product external ID (for external system references).
    /// When set, this product becomes a variant of the specified main product.
    /// </summary>
    public string? MainProductExtId { get; init; }
}
