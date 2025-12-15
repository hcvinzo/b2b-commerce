using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.DeleteAttributeDefinition;

/// <summary>
/// Command to delete an attribute definition (soft delete)
/// </summary>
public record DeleteAttributeDefinitionCommand(Guid Id) : ICommand<Result<bool>>;
