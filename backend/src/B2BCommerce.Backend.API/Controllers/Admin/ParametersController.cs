using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Parameters;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing system and business parameters
/// </summary>
[ApiController]
[Route("api/admin/parameters")]
[Authorize(Roles = "Admin")]
public class ParametersController : ControllerBase
{
    private readonly IParameterService _parameterService;

    public ParametersController(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    /// <summary>
    /// Get all parameters with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ParameterListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] ParameterType? parameterType = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _parameterService.GetAllAsync(
            page, pageSize, search, parameterType, category, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get parameter by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParameterDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _parameterService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get parameter by key
    /// </summary>
    [HttpGet("by-key/{key}")]
    [ProducesResponseType(typeof(ParameterDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken)
    {
        var result = await _parameterService.GetByKeyAsync(key, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all distinct categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _parameterService.GetCategoriesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new parameter
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ParameterDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateParameterDto request,
        CancellationToken cancellationToken)
    {
        var result = await _parameterService.CreateAsync(request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing parameter
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ParameterDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateParameterDto request,
        CancellationToken cancellationToken)
    {
        var result = await _parameterService.UpdateAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PARAMETER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a parameter
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _parameterService.DeleteAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PARAMETER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
