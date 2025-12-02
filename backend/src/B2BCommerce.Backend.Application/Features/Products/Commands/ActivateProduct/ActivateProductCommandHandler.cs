using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.ActivateProduct;

/// <summary>
/// Handler for ActivateProductCommand
/// </summary>
public class ActivateProductCommandHandler : ICommandHandler<ActivateProductCommand, Result>
{
    private readonly IProductService _productService;

    public ActivateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        return await _productService.ActivateAsync(request.Id, cancellationToken);
    }
}
