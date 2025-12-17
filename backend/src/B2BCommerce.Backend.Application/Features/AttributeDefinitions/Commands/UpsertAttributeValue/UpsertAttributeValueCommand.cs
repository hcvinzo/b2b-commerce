using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeValue;

/// <summary>
/// Command to create or update a predefined value for an attribute definition.
/// Uses Value as the upsert key within the attribute.
/// </summary>
public record UpsertAttributeValueCommand : ICommand<Result<AttributeValueDto>>
{
    /// <summary>
    /// Attribute definition internal ID (one of this or ExternalId required)
    /// </summary>
    public Guid? AttributeDefinitionId { get; init; }

    /// <summary>
    /// Attribute definition external ID (one of this or AttributeDefinitionId required)
    /// </summary>
    public string? AttributeExternalId { get; init; }

    /// <summary>
    /// The value (required, used as upsert key)
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
