using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var updateDto = new UpdateProductDto
        {
            Name = request.Name,
            Description = request.Description,
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
            Status = request.Status,
            IsSerialTracked = request.IsSerialTracked,
            TaxRate = request.TaxRate,
            MainImageUrl = request.MainImageUrl,
            ImageUrls = request.ImageUrls,
            Specifications = request.Specifications,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height
        };

        return await _productService.UpdateAsync(request.Id, updateDto, cancellationToken);
    }
}
