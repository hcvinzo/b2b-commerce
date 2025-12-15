using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpdateProductType;

/// <summary>
/// Command to update an existing product type
/// </summary>
public record UpdateProductTypeCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive) : ICommand<Result<ProductTypeDto>>;
