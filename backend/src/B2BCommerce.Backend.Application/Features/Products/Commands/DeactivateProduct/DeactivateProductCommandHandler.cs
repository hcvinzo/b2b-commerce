using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.DeactivateProduct;

/// <summary>
/// Handler for DeactivateProductCommand
/// </summary>
public class DeactivateProductCommandHandler : ICommandHandler<DeactivateProductCommand, Result>
{
    private readonly IProductService _productService;

    public DeactivateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        return await _productService.DeactivateAsync(request.Id, cancellationToken);
    }
}
