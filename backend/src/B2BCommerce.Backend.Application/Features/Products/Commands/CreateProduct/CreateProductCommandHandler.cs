using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateProductDto
        {
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            ListPrice = request.ListPrice,
            Currency = request.Currency,
            Tier1Price = request.Tier1Price,
            Tier2Price = request.Tier2Price,
            Tier3Price = request.Tier3Price,
            Tier4Price = request.Tier4Price,
            Tier5Price = request.Tier5Price,
            StockQuantity = request.StockQuantity,
            MinimumOrderQuantity = request.MinimumOrderQuantity,
            IsSerialTracked = request.IsSerialTracked,
            TaxRate = request.TaxRate,
            MainImageUrl = request.MainImageUrl,
            ImageUrls = request.ImageUrls,
            Specifications = request.Specifications,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            MainProductId = request.MainProductId
        };

        return await _productService.CreateAsync(createDto, cancellationToken);
    }
}
