using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.CreateProductType;

/// <summary>
/// Command to create a new product type
/// </summary>
public record CreateProductTypeCommand(
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    List<AddAttributeToProductTypeDto>? Attributes) : ICommand<Result<ProductTypeDto>>;
