using B2BCommerce.Backend.IntegrationAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Base controller for Integration API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the authenticated API client ID from claims
    /// </summary>
    protected Guid? GetApiClientId()
    {
        var claim = User.FindFirst("api_client_id");
        if (claim is not null && Guid.TryParse(claim.Value, out var clientId))
        {
            return clientId;
        }
        return null;
    }

    /// <summary>
    /// Gets the authenticated API key ID from claims
    /// </summary>
    protected Guid? GetApiKeyId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim is not null && Guid.TryParse(claim.Value, out var keyId))
        {
            return keyId;
        }
        return null;
    }

    /// <summary>
    /// Gets the client name from claims
    /// </summary>
    protected string? GetClientName()
    {
        return User.FindFirst("client_name")?.Value;
    }

    /// <summary>
    /// Returns a success response with data
    /// </summary>
    protected IActionResult OkResponse<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    /// <summary>
    /// Returns a created response with data
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data, string? location = null)
    {
        var response = ApiResponse<T>.Ok(data);
        if (!string.IsNullOrEmpty(location))
        {
            return Created(location, response);
        }
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Returns a paged response
    /// </summary>
    protected IActionResult PagedResponse<T>(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return Ok(PagedApiResponse<T>.Ok(data, pageNumber, pageSize, totalCount));
    }

    /// <summary>
    /// Returns a bad request response
    /// </summary>
    protected IActionResult BadRequestResponse(string message, string? errorCode = null)
    {
        return BadRequest(ApiResponse.Error(message, errorCode));
    }

    /// <summary>
    /// Returns a not found response
    /// </summary>
    protected IActionResult NotFoundResponse(string message = "Resource not found")
    {
        return NotFound(ApiResponse.Error(message, "NOT_FOUND"));
    }

    /// <summary>
    /// Returns a conflict response
    /// </summary>
    protected IActionResult ConflictResponse(string message, string? errorCode = null)
    {
        return Conflict(ApiResponse.Error(message, errorCode ?? "CONFLICT"));
    }
}
