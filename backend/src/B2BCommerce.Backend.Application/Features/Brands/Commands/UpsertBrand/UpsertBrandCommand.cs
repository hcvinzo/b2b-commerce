using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.UpsertBrand;

/// <summary>
/// Command to upsert a brand (create or update).
/// Used for external system synchronization (LOGO ERP).
/// Matches by ExternalId (primary) or Name (fallback).
/// If neither is provided, creates a new brand with auto-generated ExternalId.
/// </summary>
public record UpsertBrandCommand : ICommand<Result<BrandDto>>
{
    /// <summary>
    /// External system ID (PRIMARY upsert key for LOGO ERP)
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    public string? ExternalCode { get; init; }

    /// <summary>
    /// Brand name (required)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Brand description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Logo URL
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// Whether brand is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}
