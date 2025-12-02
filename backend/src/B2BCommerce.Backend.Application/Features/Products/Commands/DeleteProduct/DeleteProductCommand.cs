using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid Id) : ICommand<Result>;
