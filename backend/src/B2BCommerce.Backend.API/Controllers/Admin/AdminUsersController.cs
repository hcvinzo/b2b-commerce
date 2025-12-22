using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.AdminUsers;
using B2BCommerce.Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing admin users
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      User.FindFirst("userId")?.Value;
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Get all admin users with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetAllAsync(page, pageSize, isActive, search, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get admin user by ID with details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new admin user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserDto request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.CreateAsync(request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing admin user
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdminUserDto request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.UpdateAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Activate an admin user
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.ActivateAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Admin user activated successfully" });
    }

    /// <summary>
    /// Deactivate an admin user
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.DeactivateAsync(id, GetCurrentUserId(), GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            if (result.ErrorCode == "SELF_ACTION_FORBIDDEN")
            {
                return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Admin user deactivated successfully" });
    }

    /// <summary>
    /// Delete an admin user
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.DeleteAsync(id, GetCurrentUserId(), GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            if (result.ErrorCode == "SELF_ACTION_FORBIDDEN")
            {
                return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Reset password for an admin user
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.ResetPasswordAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Password reset email sent successfully" });
    }

    /// <summary>
    /// Get available roles for admin users
    /// </summary>
    [HttpGet("available-roles")]
    [ProducesResponseType(typeof(List<AvailableRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableRoles(CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetAvailableRolesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUserRolesAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Set user roles (replaces all existing roles)
    /// </summary>
    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetUserRoles(Guid id, [FromBody] SetUserRolesDto request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.SetUserRolesAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "User roles updated successfully" });
    }

    /// <summary>
    /// Get user login history (external providers)
    /// </summary>
    [HttpGet("{id:guid}/logins")]
    [ProducesResponseType(typeof(List<UserLoginDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLogins(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUserLoginsAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user claims
    /// </summary>
    [HttpGet("{id:guid}/claims")]
    [ProducesResponseType(typeof(List<UserClaimDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserClaims(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUserClaimsAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Add a claim to user
    /// </summary>
    [HttpPost("{id:guid}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUserClaim(Guid id, [FromBody] AddUserClaimDto request, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.AddUserClaimAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Claim added successfully" });
    }

    /// <summary>
    /// Remove a claim from user
    /// </summary>
    [HttpDelete("{id:guid}/claims/{claimId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserClaim(Guid id, int claimId, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.RemoveUserClaimAsync(id, claimId, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND" || result.ErrorCode == "CLAIM_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Claim removed successfully" });
    }
}
