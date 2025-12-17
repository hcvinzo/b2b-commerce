using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Application.Features.Products.Commands.ActivateProduct;
using B2BCommerce.Backend.Application.Features.Products.Commands.CreateProduct;
using B2BCommerce.Backend.Application.Features.Products.Commands.DeactivateProduct;
using B2BCommerce.Backend.Application.Features.Products.Commands.DeleteProduct;
using B2BCommerce.Backend.Application.Features.Products.Commands.UpdateProduct;
using B2BCommerce.Backend.Application.Features.Products.Queries.GetAllProducts;
using B2BCommerce.Backend.Application.Features.Products.Queries.GetProductById;
using B2BCommerce.Backend.Application.Features.Products.Queries.GetProductBySku;
using B2BCommerce.Backend.Application.Features.Products.Queries.GetProductsByCategory;
using B2BCommerce.Backend.Application.Features.Products.Queries.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllProductsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken cancellationToken)
    {
        var query = new GetProductBySkuQuery(sku);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByCategoryQuery(categoryId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(q, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.SKU,
            request.CategoryIds,
            request.BrandId,
            request.ListPrice,
            request.Currency,
            request.Tier1Price,
            request.Tier2Price,
            request.Tier3Price,
            request.Tier4Price,
            request.Tier5Price,
            request.StockQuantity,
            request.MinimumOrderQuantity,
            request.IsSerialTracked,
            request.TaxRate,
            request.MainImageUrl,
            request.ImageUrls,
            request.Specifications,
            request.Weight,
            request.Length,
            request.Width,
            request.Height,
            request.MainProductId,
            request.ProductTypeId,
            request.AttributeValues);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ValidationErrors is not null)
            {
                return BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.CategoryIds,
            request.BrandId,
            request.ListPrice,
            request.Currency,
            request.Tier1Price,
            request.Tier2Price,
            request.Tier3Price,
            request.Tier4Price,
            request.Tier5Price,
            request.StockQuantity,
            request.MinimumOrderQuantity,
            request.Status,
            request.IsSerialTracked,
            request.TaxRate,
            request.MainImageUrl,
            request.ImageUrls,
            request.Specifications,
            request.Weight,
            request.Length,
            request.Width,
            request.Height,
            request.MainProductId,
            request.ClearMainProduct,
            request.ProductTypeId,
            request.ClearProductType,
            request.AttributeValues);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PRODUCT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            if (result.ValidationErrors is not null)
            {
                return BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Activate a product
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var command = new ActivateProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Product activated successfully" });
    }

    /// <summary>
    /// Deactivate a product
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeactivateProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Product deactivated successfully" });
    }
}
