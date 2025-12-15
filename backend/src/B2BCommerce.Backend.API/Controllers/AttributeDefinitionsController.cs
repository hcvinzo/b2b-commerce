using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.AddAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.CreateAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.DeleteAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.RemoveAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpdateAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionById;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetFilterableAttributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

/// <summary>
/// Controller for managing product attribute definitions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttributeDefinitionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttributeDefinitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all attribute definitions
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetAttributeDefinitionsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get attribute definition by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAttributeDefinitionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get filterable attribute definitions (for product filtering UI)
    /// </summary>
    [HttpGet("filterable")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterable(CancellationToken cancellationToken)
    {
        var query = new GetFilterableAttributesQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new attribute definition
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAttributeDefinitionDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAttributeDefinitionCommand(
            request.Code,
            request.Name,
            request.Type,
            request.Unit,
            request.IsFilterable,
            request.IsRequired,
            request.IsVisibleOnProductPage,
            request.DisplayOrder,
            request.PredefinedValues);

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
    /// Update an existing attribute definition
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAttributeDefinitionDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAttributeDefinitionCommand(
            id,
            request.Name,
            request.Unit,
            request.IsFilterable,
            request.IsRequired,
            request.IsVisibleOnProductPage,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ATTRIBUTE_NOT_FOUND")
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
    /// Delete an attribute definition
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAttributeDefinitionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Add a predefined value to an attribute definition (for Select/MultiSelect types)
    /// </summary>
    [HttpPost("{id:guid}/values")]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddValue(
        Guid id,
        [FromBody] AddAttributeValueDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddAttributeValueCommand(
            id,
            request.Value,
            request.DisplayText,
            request.DisplayOrder);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ATTRIBUTE_NOT_FOUND")
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
    /// Remove a predefined value from an attribute definition
    /// </summary>
    [HttpDelete("{attributeDefinitionId:guid}/values/{valueId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveValue(
        Guid attributeDefinitionId,
        Guid valueId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAttributeValueCommand(attributeDefinitionId, valueId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
