using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.CustomerUsers;
using B2BCommerce.Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing customer (dealer) users from the admin panel
/// </summary>
[ApiController]
[Route("api/admin/customers/{customerId:guid}/users")]
[Authorize(Roles = "Admin")]
public class CustomerUsersController : ControllerBase
{
    private readonly ICustomerUserService _customerUserService;

    public CustomerUsersController(ICustomerUserService customerUserService)
    {
        _customerUserService = customerUserService;
    }

    /// <summary>
    /// Get all users for a customer with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerUserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerUserService.GetUsersAsync(customerId, page, pageSize, search, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CUSTOMER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a specific customer user by ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(CustomerUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid customerId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.GetUserByIdAsync(customerId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new user for a customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerUserDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid customerId, [FromBody] CreateCustomerUserDto request, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.CreateUserAsync(customerId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CUSTOMER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetById), new { customerId, userId = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update a customer user
    /// </summary>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(CustomerUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid customerId, Guid userId, [FromBody] UpdateCustomerUserDto request, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.UpdateUserAsync(customerId, userId, request, cancellationToken);

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
    /// Activate a customer user
    /// </summary>
    [HttpPost("{userId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid customerId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.ActivateUserAsync(customerId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Customer user activated successfully" });
    }

    /// <summary>
    /// Deactivate a customer user
    /// </summary>
    [HttpPost("{userId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid customerId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.DeactivateUserAsync(customerId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Customer user deactivated successfully" });
    }

    /// <summary>
    /// Set roles for a customer user (replaces all existing roles)
    /// </summary>
    [HttpPut("{userId:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetRoles(Guid customerId, Guid userId, [FromBody] SetCustomerUserRolesDto request, CancellationToken cancellationToken)
    {
        var result = await _customerUserService.SetUserRolesAsync(customerId, userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Customer user roles updated successfully" });
    }

    /// <summary>
    /// Get available customer roles
    /// </summary>
    [HttpGet("/api/admin/customer-roles")]
    [ProducesResponseType(typeof(List<CustomerUserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableRoles(CancellationToken cancellationToken)
    {
        var result = await _customerUserService.GetAvailableRolesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }
}
