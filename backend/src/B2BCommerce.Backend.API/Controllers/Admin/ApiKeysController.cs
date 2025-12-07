using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.AddIpWhitelist;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.CreateApiKey;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RemoveIpWhitelist;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RevokeApiKey;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RotateApiKey;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdateApiKey;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdatePermissions;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeyById;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeysByClient;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageLogs;
using B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageStats;
using B2BCommerce.Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing API keys (Integration API)
/// </summary>
[ApiController]
[Route("api/admin/integration/keys")]
[Authorize(Roles = "Admin")]
public class ApiKeysController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiKeysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    /// <summary>
    /// Get all API keys for a client
    /// </summary>
    [HttpGet("by-client/{clientId:guid}")]
    [ProducesResponseType(typeof(List<ApiKeyListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClient(Guid clientId, CancellationToken cancellationToken)
    {
        var query = new GetApiKeysByClientQuery(clientId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get API key by ID with details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiKeyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetApiKeyByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new API key
    /// </summary>
    /// <remarks>
    /// IMPORTANT: The plain text API key is only returned once during creation.
    /// Make sure to securely store it as it cannot be retrieved again.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApiKeyResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyDto request, CancellationToken cancellationToken)
    {
        var command = new CreateApiKeyCommand(
            request.ApiClientId,
            request.Name,
            request.RateLimitPerMinute,
            request.ExpiresAt,
            request.Permissions,
            request.IpWhitelist,
            GetCurrentUser());

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ValidationErrors != null)
            {
                return BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing API key
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiKeyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApiKeyDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateApiKeyCommand(
            id,
            request.Name,
            request.RateLimitPerMinute,
            request.ExpiresAt,
            GetCurrentUser());

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            if (result.ValidationErrors != null)
            {
                return BadRequest(new { message = result.ErrorMessage, errors = result.ValidationErrors });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Revoke an API key
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(Guid id, [FromBody] RevokeApiKeyDto request, CancellationToken cancellationToken)
    {
        var command = new RevokeApiKeyCommand(id, request.Reason, GetCurrentUser());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "API key revoked successfully" });
    }

    /// <summary>
    /// Rotate an API key (creates new key, revokes old)
    /// </summary>
    /// <remarks>
    /// IMPORTANT: The plain text API key is only returned once during rotation.
    /// Make sure to securely store it as it cannot be retrieved again.
    /// </remarks>
    [HttpPost("{id:guid}/rotate")]
    [ProducesResponseType(typeof(CreateApiKeyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rotate(Guid id, CancellationToken cancellationToken)
    {
        var command = new RotateApiKeyCommand(id, GetCurrentUser());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update API key permissions
    /// </summary>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdateApiKeyPermissionsDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateApiKeyPermissionsCommand(id, request.Permissions);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Permissions updated successfully" });
    }

    /// <summary>
    /// Add IP address to whitelist
    /// </summary>
    [HttpPost("{id:guid}/ip-whitelist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddIpWhitelist(Guid id, [FromBody] AddIpWhitelistDto request, CancellationToken cancellationToken)
    {
        var command = new AddIpWhitelistCommand(id, request.IpAddress, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "IP address added to whitelist" });
    }

    /// <summary>
    /// Remove IP address from whitelist
    /// </summary>
    [HttpDelete("{id:guid}/ip-whitelist/{whitelistId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveIpWhitelist(Guid id, Guid whitelistId, CancellationToken cancellationToken)
    {
        var command = new RemoveIpWhitelistCommand(id, whitelistId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "KEY_NOT_FOUND" || result.ErrorCode == "IP_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "IP address removed from whitelist" });
    }

    /// <summary>
    /// Get usage logs for an API key
    /// </summary>
    [HttpGet("{id:guid}/usage")]
    [ProducesResponseType(typeof(PagedResult<ApiKeyUsageLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsageLogs(
        Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApiKeyUsageLogsQuery(
            id,
            fromDate ?? DateTime.UtcNow.AddDays(-7),
            toDate ?? DateTime.UtcNow,
            page,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get usage statistics for an API key
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    [ProducesResponseType(typeof(ApiKeyUsageStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsageStats(
        Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApiKeyUsageStatsQuery(
            id,
            fromDate ?? DateTime.UtcNow.AddDays(-30),
            toDate ?? DateTime.UtcNow);

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get available permission scopes
    /// </summary>
    [HttpGet("available-scopes")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAvailableScopes()
    {
        return Ok(new
        {
            scopes = IntegrationPermissionScopes.GetAllScopes(),
            categories = new[]
            {
                new { name = "Products", scopes = new[] { "products:read", "products:write" } },
                new { name = "Stock", scopes = new[] { "stock:read", "stock:write" } },
                new { name = "Orders", scopes = new[] { "orders:read", "orders:write" } },
                new { name = "Customers", scopes = new[] { "customers:read", "customers:write" } },
                new { name = "Prices", scopes = new[] { "prices:read", "prices:write" } },
                new { name = "Full Access", scopes = new[] { "*" } }
            }
        });
    }
}
