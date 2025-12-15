using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.AddAttributeToProductType;

/// <summary>
/// Command to add an attribute definition to a product type
/// </summary>
public record AddAttributeToProductTypeCommand(
    Guid ProductTypeId,
    Guid AttributeDefinitionId,
    bool IsRequired,
    int DisplayOrder) : ICommand<Result<ProductTypeDto>>;
