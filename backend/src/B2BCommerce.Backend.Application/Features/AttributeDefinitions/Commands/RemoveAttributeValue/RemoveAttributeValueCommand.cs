using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.RemoveAttributeValue;

/// <summary>
/// Command to remove a predefined value from an attribute definition
/// </summary>
public record RemoveAttributeValueCommand(
    Guid AttributeDefinitionId,
    Guid AttributeValueId) : ICommand<Result<bool>>;
