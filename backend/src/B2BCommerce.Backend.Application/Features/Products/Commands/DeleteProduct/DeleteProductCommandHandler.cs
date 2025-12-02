using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        return await _productService.DeleteAsync(request.Id, cancellationToken);
    }
}
