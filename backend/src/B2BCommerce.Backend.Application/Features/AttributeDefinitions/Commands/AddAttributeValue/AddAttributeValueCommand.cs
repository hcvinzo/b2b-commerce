using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.AddAttributeValue;

/// <summary>
/// Command to add a predefined value to an attribute definition
/// </summary>
public record AddAttributeValueCommand(
    Guid AttributeDefinitionId,
    string Value,
    string? DisplayText,
    int DisplayOrder) : ICommand<Result<AttributeValueDto>>;
