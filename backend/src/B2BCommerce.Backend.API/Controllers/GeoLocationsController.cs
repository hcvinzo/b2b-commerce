using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Features.GeoLocations.Commands.CreateGeoLocation;
using B2BCommerce.Backend.Application.Features.GeoLocations.Commands.DeleteGeoLocation;
using B2BCommerce.Backend.Application.Features.GeoLocations.Commands.UpdateGeoLocation;
using B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationById;
using B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocations;
using B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByParent;
using B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByType;
using B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationTree;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeoLocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GeoLocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all geo locations with pagination and filtering
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<GeoLocationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? typeId,
        [FromQuery] Guid? parentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetGeoLocationsQuery(
            search,
            typeId,
            parentId,
            pageNumber,
            pageSize,
            sortBy,
            sortDirection);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get geo location by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GeoLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get geo location tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GeoLocationTreeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree(
        [FromQuery] Guid? typeId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetGeoLocationTreeQuery(typeId), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get geo locations by type
    /// </summary>
    [HttpGet("by-type/{typeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GeoLocationListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByType(Guid typeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationsByTypeQuery(typeId), cancellationToken);

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
    /// Get root geo locations (no parent)
    /// </summary>
    [HttpGet("root")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GeoLocationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoot(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationsByParentQuery(null), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get children of a geo location
    /// </summary>
    [HttpGet("{parentId:guid}/children")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GeoLocationListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildren(Guid parentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGeoLocationsByParentQuery(parentId), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PARENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new geo location
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GeoLocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGeoLocationDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var command = new CreateGeoLocationCommand(
            request.GeoLocationTypeId,
            request.Code,
            request.Name,
            request.ParentId,
            request.Latitude,
            request.Longitude,
            request.Metadata,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "GEO_LOCATION_TYPE_NOT_FOUND" || result.ErrorCode == "PARENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update a geo location
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GeoLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGeoLocationDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var command = new UpdateGeoLocationCommand(
            id,
            request.Code,
            request.Name,
            request.Latitude,
            request.Longitude,
            request.Metadata,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "GEO_LOCATION_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a geo location
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new DeleteGeoLocationCommand(id, userId), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "GEO_LOCATION_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
