using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.ActivateApiClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.CreateApiClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeactivateApiClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.DeleteApiClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Commands.UpdateApiClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetAllApiClients;
using B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetApiClientById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing API clients (Integration API)
/// </summary>
[ApiController]
[Route("api/admin/integration/clients")]
[Authorize(Roles = "Admin")]
public class ApiClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    /// <summary>
    /// Get all API clients with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApiClientListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllApiClientsQuery(page, pageSize, isActive);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get API client by ID with details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiClientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetApiClientByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new API client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiClientDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiClientDto request, CancellationToken cancellationToken)
    {
        var command = new CreateApiClientCommand(
            request.Name,
            request.Description,
            request.ContactEmail,
            request.ContactPhone,
            GetCurrentUser());

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
    /// Update an existing API client
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiClientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApiClientDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateApiClientCommand(
            id,
            request.Name,
            request.Description,
            request.ContactEmail,
            request.ContactPhone,
            GetCurrentUser());

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CLIENT_NOT_FOUND")
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
    /// Activate an API client
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var command = new ActivateApiClientCommand(id, GetCurrentUser());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CLIENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "API client activated successfully" });
    }

    /// <summary>
    /// Deactivate an API client
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeactivateApiClientCommand(id, GetCurrentUser());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CLIENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "API client deactivated successfully" });
    }

    /// <summary>
    /// Delete an API client (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteApiClientCommand(id, GetCurrentUser());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CLIENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
