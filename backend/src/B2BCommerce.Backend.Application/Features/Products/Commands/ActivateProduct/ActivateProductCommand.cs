using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.ActivateProduct;

/// <summary>
/// Command to activate a product
/// </summary>
public record ActivateProductCommand(Guid Id) : ICommand<Result>;
