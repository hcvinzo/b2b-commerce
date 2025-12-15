using System.Security.Claims;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Features.Orders.Commands.AddOrderItem;
using B2BCommerce.Backend.Application.Features.Orders.Commands.ApproveOrder;
using B2BCommerce.Backend.Application.Features.Orders.Commands.CancelOrder;
using B2BCommerce.Backend.Application.Features.Orders.Commands.CreateOrder;
using B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsDelivered;
using B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsProcessing;
using B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsShipped;
using B2BCommerce.Backend.Application.Features.Orders.Commands.RejectOrder;
using B2BCommerce.Backend.Application.Features.Orders.Commands.RemoveOrderItem;
using B2BCommerce.Backend.Application.Features.Orders.Commands.UpdateOrderItemQuantity;
using B2BCommerce.Backend.Application.Features.Orders.Queries.GetAllOrders;
using B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrderById;
using B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrderByNumber;
using B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrdersByCustomer;
using B2BCommerce.Backend.Application.Features.Orders.Queries.GetPendingOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllOrdersQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var query = new GetOrderByNumberQuery(orderNumber);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get orders for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(
        Guid customerId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByCustomerQuery(customerId, status);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get current customer's orders
    /// </summary>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var customerIdClaim = User.FindFirst("customerId")?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return BadRequest(new { message = "Customer ID not found in token", code = "INVALID_CUSTOMER" });
        }

        var query = new GetOrdersByCustomerQuery(customerId, status);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get orders pending approval
    /// </summary>
    [HttpGet("pending-approval")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOrders(CancellationToken cancellationToken)
    {
        var query = new GetPendingOrdersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingCountry,
            request.ShippingPostalCode,
            request.Currency,
            request.CustomerNote,
            request.ShippingNote,
            request.OrderItems,
            request.DiscountAmount,
            request.ShippingCost);

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
    /// Add item to an existing order
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] CreateOrderItemDto request, CancellationToken cancellationToken)
    {
        var command = new AddOrderItemCommand(id, request.ProductId, request.Quantity, request.DiscountAmount);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update order item quantity
    /// </summary>
    [HttpPut("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity(
        Guid orderId,
        Guid itemId,
        [FromBody] UpdateQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderItemQuantityCommand(orderId, itemId, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND" || result.ErrorCode == "ITEM_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Remove item from order
    /// </summary>
    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new RemoveOrderItemCommand(orderId, itemId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Approve an order
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveOrderRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var approvedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        var command = new ApproveOrderCommand(id, approvedBy, request?.ExchangeRate);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Reject an order
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectOrderRequest request, CancellationToken cancellationToken)
    {
        var rejectedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        var command = new RejectOrderCommand(id, rejectedBy, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Mark order as processing
    /// </summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsProcessing(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkAsProcessingCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Mark order as shipped
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsShipped(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkAsShippedCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Mark order as delivered
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDelivered(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkAsDeliveredCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "ORDER_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }
}

/// <summary>
/// Request model for updating item quantity
/// </summary>
public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for approving an order
/// </summary>
public class ApproveOrderRequest
{
    public decimal? ExchangeRate { get; set; }
}

/// <summary>
/// Request model for rejecting an order
/// </summary>
public class RejectOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}
