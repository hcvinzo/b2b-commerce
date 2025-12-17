using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;

/// <summary>
/// Command to upsert a category (create or update).
/// Used for external system synchronization
/// Matches by ExternalId (primary) or ExternalCode (fallback), or by Id for internal updates.
/// </summary>
public record UpsertCategoryCommand : ICommand<Result<CategoryDto>>
{
    /// <summary>
    /// Internal ID (for internal updates)
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// External system ID
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    public string? ExternalCode { get; init; }

    /// <summary>
    /// Category name (required)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Parent category internal ID
    /// </summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>
    /// Parent category external ID (for external system references - PRIMARY)
    /// </summary>
    public string? ParentExternalId { get; init; }

    /// <summary>
    /// Parent category external code (for external system references - FALLBACK)
    /// </summary>
    public string? ParentExternalCode { get; init; }

    /// <summary>
    /// Image URL
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; init; } = 0;

    /// <summary>
    /// Active status
    /// </summary>
    public bool IsActive { get; init; } = true;
}
