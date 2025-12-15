using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.DeleteProductType;

/// <summary>
/// Command to delete a product type (soft delete)
/// </summary>
public record DeleteProductTypeCommand(Guid Id) : ICommand<Result<bool>>;
