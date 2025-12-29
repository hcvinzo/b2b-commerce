using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.CreateGeoLocationType;
using B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.DeleteGeoLocationType;
using B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.UpdateGeoLocationType;
using B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypeById;
using B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeoLocationTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GeoLocationTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all geo location types
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GeoLocationTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationTypesQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get geo location type by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GeoLocationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationTypeByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new geo location type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GeoLocationTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGeoLocationTypeDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var command = new CreateGeoLocationTypeCommand(
            request.Name,
            request.DisplayOrder,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update a geo location type
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GeoLocationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGeoLocationTypeDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var command = new UpdateGeoLocationTypeCommand(
            id,
            request.Name,
            request.DisplayOrder,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "GEO_LOCATION_TYPE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a geo location type
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new DeleteGeoLocationTypeCommand(id, userId), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "GEO_LOCATION_TYPE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
