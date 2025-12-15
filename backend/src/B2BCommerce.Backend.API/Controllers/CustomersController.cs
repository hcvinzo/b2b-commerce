using System.Security.Claims;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Features.Customers.Commands.ActivateCustomer;
using B2BCommerce.Backend.Application.Features.Customers.Commands.ApproveCustomer;
using B2BCommerce.Backend.Application.Features.Customers.Commands.DeactivateCustomer;
using B2BCommerce.Backend.Application.Features.Customers.Commands.DeleteCustomer;
using B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCreditLimit;
using B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCustomer;
using B2BCommerce.Backend.Application.Features.Customers.Queries.GetAllCustomers;
using B2BCommerce.Backend.Application.Features.Customers.Queries.GetAvailableCredit;
using B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerByEmail;
using B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerById;
using B2BCommerce.Backend.Application.Features.Customers.Queries.GetUnapprovedCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all customers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllCustomersQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByEmailQuery(email);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get current customer profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentCustomer(CancellationToken cancellationToken)
    {
        var customerIdClaim = User.FindFirst("customerId")?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return NotFound(new { message = "Customer not found", code = "CUSTOMER_NOT_FOUND" });
        }

        var query = new GetCustomerByIdQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get customers pending approval
    /// </summary>
    [HttpGet("pending-approval")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnapproved(CancellationToken cancellationToken)
    {
        var query = new GetUnapprovedCustomersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get customer's available credit
    /// </summary>
    [HttpGet("{id:guid}/available-credit")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableCredit(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAvailableCreditQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { availableCredit = result.Data });
    }

    /// <summary>
    /// Update customer information
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.CompanyName,
            request.Phone,
            request.ContactPersonName,
            request.ContactPersonTitle,
            request.BillingStreet,
            request.BillingCity,
            request.BillingState,
            request.BillingCountry,
            request.BillingPostalCode,
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingCountry,
            request.ShippingPostalCode,
            request.PreferredLanguage);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CUSTOMER_NOT_FOUND")
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
    /// Approve a customer account
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var approvedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        var command = new ApproveCustomerCommand(id, approvedBy);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update customer credit limit
    /// </summary>
    [HttpPut("{id:guid}/credit-limit")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCreditLimit(
        Guid id,
        [FromBody] UpdateCreditLimitRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCreditLimitCommand(id, request.NewCreditLimit);
        var result = await _mediator.Send(command, cancellationToken);

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
    /// Activate a customer account
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var command = new ActivateCustomerCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Customer activated successfully" });
    }

    /// <summary>
    /// Deactivate a customer account
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeactivateCustomerCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Customer deactivated successfully" });
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for updating credit limit
/// </summary>
public class UpdateCreditLimitRequest
{
    public decimal NewCreditLimit { get; set; }
}
