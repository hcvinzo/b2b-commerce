using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.AddAttributeToProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.CreateProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.DeleteProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.RemoveAttributeFromProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpdateProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeByCode;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeById;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

/// <summary>
/// Controller for managing product types (attribute templates)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all product types
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductTypeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductTypesQuery(isActive);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get product type by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductTypeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get product type by code
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var query = new GetProductTypeByCodeQuery(code);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new product type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductTypeCommand(
            request.Code,
            request.Name,
            request.Description,
            true,
            request.Attributes);

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
    /// Update an existing product type
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductTypeCommand(
            id,
            request.Name,
            request.Description,
            request.IsActive);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PRODUCT_TYPE_NOT_FOUND")
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
    /// Delete a product type
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductTypeCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Add an attribute to a product type
    /// </summary>
    [HttpPost("{id:guid}/attributes")]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAttribute(
        Guid id,
        [FromBody] AddAttributeToProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddAttributeToProductTypeCommand(
            id,
            request.AttributeDefinitionId,
            request.IsRequired,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode is "PRODUCT_TYPE_NOT_FOUND" or "ATTRIBUTE_NOT_FOUND")
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
    /// Remove an attribute from a product type
    /// </summary>
    [HttpDelete("{productTypeId:guid}/attributes/{attributeDefinitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAttribute(
        Guid productTypeId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAttributeFromProductTypeCommand(productTypeId, attributeDefinitionId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
