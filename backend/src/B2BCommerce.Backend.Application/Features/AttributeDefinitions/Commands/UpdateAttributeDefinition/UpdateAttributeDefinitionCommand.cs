using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpdateAttributeDefinition;

/// <summary>
/// Command to update an existing attribute definition
/// </summary>
public record UpdateAttributeDefinitionCommand(
    Guid Id,
    string Name,
    string? NameEn,
    string? Unit,
    bool IsFilterable,
    bool IsRequired,
    bool IsVisibleOnProductPage,
    int DisplayOrder) : ICommand<Result<AttributeDefinitionDto>>;
