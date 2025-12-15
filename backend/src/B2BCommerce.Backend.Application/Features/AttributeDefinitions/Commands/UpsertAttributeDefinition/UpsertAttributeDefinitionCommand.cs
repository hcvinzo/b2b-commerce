using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeDefinition;

/// <summary>
/// Command to create or update an attribute definition from external system.
/// Uses ExternalId as the primary upsert key.
/// </summary>
public record UpsertAttributeDefinitionCommand : ICommand<Result<AttributeDefinitionDto>>
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
    /// Unique code for the attribute (required)
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Display name in Turkish (required)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Display name in English (optional)
    /// </summary>
    public string? NameEn { get; init; }

    /// <summary>
    /// Data type for this attribute (required)
    /// </summary>
    public required AttributeType Type { get; init; }

    /// <summary>
    /// Unit of measurement (optional)
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// Whether this attribute should appear in product filters
    /// </summary>
    public bool IsFilterable { get; init; } = true;

    /// <summary>
    /// Default required status
    /// </summary>
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; init; } = true;

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; init; } = 0;

    /// <summary>
    /// Predefined values for Select/MultiSelect types (full replacement)
    /// </summary>
    public List<UpsertAttributeValueDto>? PredefinedValues { get; init; }

    /// <summary>
    /// User/client performing the operation
    /// </summary>
    public string? ModifiedBy { get; init; }
}

/// <summary>
/// DTO for predefined value in upsert operation
/// </summary>
public record UpsertAttributeValueDto
{
    /// <summary>
    /// The value (used as key for matching existing values)
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// User-facing display text
    /// </summary>
    public string? DisplayText { get; init; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; init; } = 0;
}
