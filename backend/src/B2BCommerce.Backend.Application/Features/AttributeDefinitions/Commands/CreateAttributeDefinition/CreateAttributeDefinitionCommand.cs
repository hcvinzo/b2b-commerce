using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.CreateAttributeDefinition;

/// <summary>
/// Command to create a new attribute definition
/// </summary>
public record CreateAttributeDefinitionCommand(
    string Code,
    string Name,
    AttributeType Type,
    string? Unit,
    bool IsFilterable,
    bool IsRequired,
    bool IsVisibleOnProductPage,
    int DisplayOrder,
    List<CreateAttributeValueDto>? PredefinedValues) : ICommand<Result<AttributeDefinitionDto>>;
