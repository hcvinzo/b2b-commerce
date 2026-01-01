using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Roles;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing roles and permissions
/// </summary>
[ApiController]
[Route("api/admin/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService;

    public RolesController(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    #region Role CRUD

    /// <summary>
    /// Get all roles with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RoleListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] UserType? userType = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetAllRolesAsync(page, pageSize, search, userType, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get role by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetRoleByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.CreateRoleAsync(request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.UpdateRoleAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.DeleteRoleAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }

    #endregion

    #region Available Permissions

    /// <summary>
    /// Get all available permissions grouped by category
    /// </summary>
    [HttpGet("available-permissions")]
    [ProducesResponseType(typeof(List<PermissionCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePermissions(CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetAvailablePermissionsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    #endregion

    #region Role Claims

    /// <summary>
    /// Get all claims for a specific role
    /// </summary>
    [HttpGet("{id:guid}/claims")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleClaims(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetRoleClaimsAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Set all claims for a role (replaces existing claims)
    /// </summary>
    [HttpPut("{id:guid}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoleClaims(Guid id, [FromBody] SetRoleClaimsDto request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.SetRoleClaimsAsync(id, request.Claims, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Role claims updated successfully" });
    }

    /// <summary>
    /// Add a single claim to a role
    /// </summary>
    [HttpPost("{id:guid}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddClaim(Guid id, [FromBody] RoleClaimDto request, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.AddClaimToRoleAsync(id, request.ClaimValue, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Claim added to role successfully" });
    }

    /// <summary>
    /// Remove a claim from a role
    /// </summary>
    [HttpDelete("{id:guid}/claims/{claimValue}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveClaim(Guid id, string claimValue, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.RemoveClaimFromRoleAsync(id, claimValue, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND" || result.ErrorCode == "CLAIM_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Claim removed from role successfully" });
    }

    #endregion

    #region Role Users

    /// <summary>
    /// Get all users in a specific role
    /// </summary>
    [HttpGet("{id:guid}/users")]
    [ProducesResponseType(typeof(PagedResult<RoleUserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsersInRole(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleManagementService.GetUsersInRoleAsync(id, page, pageSize, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Add a user to a role
    /// </summary>
    [HttpPost("{id:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddUserToRole(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.AddUserToRoleAsync(id, userId, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND" || result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "User added to role successfully" });
    }

    /// <summary>
    /// Remove a user from a role
    /// </summary>
    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserFromRole(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.RemoveUserFromRoleAsync(id, userId, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ROLE_NOT_FOUND" || result.ErrorCode == "USER_NOT_FOUND" || result.ErrorCode == "USER_NOT_IN_ROLE")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "User removed from role successfully" });
    }

    #endregion
}
