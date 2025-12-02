using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.DeactivateProduct;

/// <summary>
/// Command to deactivate a product
/// </summary>
public record DeactivateProductCommand(Guid Id) : ICommand<Result>;
