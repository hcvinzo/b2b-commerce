using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.RemoveAttributeFromProductType;

/// <summary>
/// Command to remove an attribute definition from a product type
/// </summary>
public record RemoveAttributeFromProductTypeCommand(
    Guid ProductTypeId,
    Guid AttributeDefinitionId) : ICommand<Result<ProductTypeDto>>;
