using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpsertProductType;

/// <summary>
/// Command to create or update a product type from external system.
/// Uses ExternalId as the primary upsert key.
/// </summary>
public record UpsertProductTypeCommand : ICommand<Result<ProductTypeDto>>
{
    /// <summary>
    /// Internal ID (optional for upsert - if provided without ExternalId, creates with this ID)
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// External system ID (PRIMARY upsert key)
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    public string? ExternalCode { get; init; }

    /// <summary>
    /// Unique code for the product type (required)
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Display name (required)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Admin description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether new products can use this type
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Attributes to assign to this product type (full replacement)
    /// </summary>
    public List<UpsertProductTypeAttributeDto>? Attributes { get; init; }
}

/// <summary>
/// DTO for product type attribute in upsert operation
/// </summary>
public record UpsertProductTypeAttributeDto
{
    /// <summary>
    /// Attribute definition ID (optional if ExternalId provided)
    /// </summary>
    public Guid? AttributeDefinitionId { get; init; }

    /// <summary>
    /// Attribute definition external ID (for lookup by external system)
    /// </summary>
    public string? AttributeExternalId { get; init; }

    /// <summary>
    /// Attribute code (for lookup by code)
    /// </summary>
    public string? AttributeCode { get; init; }

    /// <summary>
    /// Whether this attribute is required for this product type
    /// </summary>
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// Display order within this product type
    /// </summary>
    public int DisplayOrder { get; init; } = 0;
}
