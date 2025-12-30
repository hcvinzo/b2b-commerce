using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Order service implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        return Result<OrderDto>.Success(MapToOrderDto(order));
    }

    public async Task<Result<OrderDto>> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        return Result<OrderDto>.Success(MapToOrderDto(order));
    }

    public async Task<Result<PagedResult<OrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        var totalCount = orders.Count();

        var pagedOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToOrderDto)
            .ToList();

        var pagedResult = new PagedResult<OrderDto>(pagedOrders, pageNumber, pageSize, totalCount);
        return Result<PagedResult<OrderDto>>.Success(pagedResult);
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetByCustomerAsync(Guid customerId, string? status = null, CancellationToken cancellationToken = default)
    {
        OrderStatus? orderStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
        {
            orderStatus = parsedStatus;
        }

        var orders = await _unitOfWork.Orders.GetByCustomerAsync(customerId, orderStatus, cancellationToken);
        var orderDtos = orders.Select(MapToOrderDto);
        return Result<IEnumerable<OrderDto>>.Success(orderDtos);
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetPendingOrdersAsync(cancellationToken);
        var orderDtos = orders.Select(MapToOrderDto);
        return Result<IEnumerable<OrderDto>>.Success(orderDtos);
    }

    public async Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        // Validate customer
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        if (customer.Status != CustomerStatus.Active)
        {
            return Result<OrderDto>.Failure("Customer is not approved to place orders", "CUSTOMER_NOT_APPROVED");
        }

        if (dto.OrderItems is null || !dto.OrderItems.Any())
        {
            return Result<OrderDto>.Failure("Order must have at least one item", "NO_ORDER_ITEMS");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create order
            var order = new Order(
                customerId: dto.CustomerId,
                shippingAddress: new Address(
                    dto.ShippingStreet,
                    dto.ShippingCity,
                    dto.ShippingState,
                    dto.ShippingCountry,
                    dto.ShippingPostalCode),
                currency: dto.Currency,
                customerNote: dto.CustomerNote
            );

            // Add order items
            foreach (var item in dto.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure($"Product not found: {item.ProductId}", "PRODUCT_NOT_FOUND");
                }

                if (!product.IsActive)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure($"Product is not available: {product.Name}", "PRODUCT_INACTIVE");
                }

                if (item.Quantity < product.MinimumOrderQuantity)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure(
                        $"Minimum order quantity for {product.Name} is {product.MinimumOrderQuantity}",
                        "MIN_ORDER_QUANTITY");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<OrderDto>.Failure(
                        $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}",
                        "INSUFFICIENT_STOCK");
                }

                // Get price (price tier is now handled via dynamic attributes, use base price for now)
                var unitPrice = product.ListPrice;

                Money? discountAmount = null;
                if (item.DiscountAmount.HasValue && item.DiscountAmount.Value > 0)
                {
                    discountAmount = new Money(item.DiscountAmount.Value, dto.Currency);
                }

                order.AddOrderItem(
                    productId: product.Id,
                    productName: product.Name,
                    productSKU: product.SKU,
                    quantity: item.Quantity,
                    unitPrice: unitPrice,
                    taxRate: product.TaxRate,
                    discount: discountAmount);

                // Reserve stock
                product.ReserveStock(item.Quantity);
                _unitOfWork.Products.Update(product);
            }

            // Apply order-level discount
            if (dto.DiscountAmount.HasValue && dto.DiscountAmount.Value > 0)
            {
                order.ApplyDiscount(new Money(dto.DiscountAmount.Value, dto.Currency));
            }

            // Set shipping cost
            if (dto.ShippingCost.HasValue && dto.ShippingCost.Value > 0)
            {
                order.SetShippingCost(new Money(dto.ShippingCost.Value, dto.Currency));
            }

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Order created with ID {OrderId} and number {OrderNumber}",
                order.Id, order.OrderNumber);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (OutOfStockException ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Stock validation failed during order creation");
            return Result<OrderDto>.Failure(ex.Message, "INSUFFICIENT_STOCK");
        }
        catch (InvalidOperationDomainException ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Domain validation failed during order creation");
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error creating order");
            return Result<OrderDto>.Failure("Failed to create order", "CREATE_FAILED");
        }
    }

    public async Task<Result<OrderDto>> AddOrderItemAsync(Guid orderId, CreateOrderItemDto item, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<OrderDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<OrderDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            // Price tier is now handled via dynamic attributes, use base price for now
            var unitPrice = product.ListPrice;

            Money? discountAmount = null;
            if (item.DiscountAmount.HasValue && item.DiscountAmount.Value > 0)
            {
                discountAmount = new Money(item.DiscountAmount.Value, order.Subtotal.Currency);
            }

            order.AddOrderItem(
                productId: product.Id,
                productName: product.Name,
                productSKU: product.SKU,
                quantity: item.Quantity,
                unitPrice: unitPrice,
                taxRate: product.TaxRate,
                discount: discountAmount);

            product.ReserveStock(item.Quantity);

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order item added to order {OrderId}", orderId);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot add item to order {OrderId}", orderId);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> RemoveOrderItemAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        var orderItem = order.OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (orderItem is null)
        {
            return Result<OrderDto>.Failure("Order item not found", "ORDER_ITEM_NOT_FOUND");
        }

        try
        {
            // Release stock
            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId, cancellationToken);
            if (product is not null)
            {
                product.ReleaseStock(orderItem.Quantity);
                _unitOfWork.Products.Update(product);
            }

            order.RemoveOrderItem(orderItemId);

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order item {OrderItemId} removed from order {OrderId}", orderItemId, orderId);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot remove item from order {OrderId}", orderId);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderItemId, int newQuantity, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        var orderItem = order.OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (orderItem is null)
        {
            return Result<OrderDto>.Failure("Order item not found", "ORDER_ITEM_NOT_FOUND");
        }

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId, cancellationToken);
            if (product is not null)
            {
                var quantityDifference = newQuantity - orderItem.Quantity;
                if (quantityDifference > 0)
                {
                    // Need more stock
                    product.ReserveStock(quantityDifference);
                }
                else if (quantityDifference < 0)
                {
                    // Release excess stock
                    product.ReleaseStock(-quantityDifference);
                }
                _unitOfWork.Products.Update(product);
            }

            order.UpdateOrderItemQuantity(orderItemId, newQuantity);

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order item {OrderItemId} quantity updated to {Quantity}", orderItemId, newQuantity);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot update item quantity in order {OrderId}", orderId);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
        catch (OutOfStockException ex)
        {
            _logger.LogWarning(ex, "Insufficient stock for order item update");
            return Result<OrderDto>.Failure(ex.Message, "INSUFFICIENT_STOCK");
        }
    }

    public async Task<Result<OrderDto>> ApproveAsync(Guid id, string approvedBy, decimal? exchangeRate = null, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            // Credit management is now handled via dynamic attributes
            // TODO: Implement credit check via customer attributes if needed

            order.Approve(approvedBy, exchangeRate);

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} approved by {ApprovedBy}", id, approvedBy);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot approve order {OrderId}", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
        catch (InsufficientCreditException ex)
        {
            _logger.LogWarning(ex, "Insufficient credit for order approval");
            return Result<OrderDto>.Failure(ex.Message, "INSUFFICIENT_CREDIT");
        }
    }

    public async Task<Result<OrderDto>> RejectAsync(Guid id, string rejectedBy, string reason, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            order.Reject(rejectedBy, reason);

            // Release reserved stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is not null)
                {
                    product.ReleaseStock(item.Quantity);
                    _unitOfWork.Products.Update(product);
                }
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} rejected by {RejectedBy}: {Reason}", id, rejectedBy, reason);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot reject order {OrderId}", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            order.Cancel();

            // Release reserved stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is not null)
                {
                    product.ReleaseStock(item.Quantity);
                    _unitOfWork.Products.Update(product);
                }
            }

            // Credit management is now handled via dynamic attributes
            // TODO: Implement credit release via customer attributes if needed

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} cancelled", id);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel order {OrderId}", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> MarkAsProcessingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            order.MarkAsProcessing();

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as processing", id);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot mark order {OrderId} as processing", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            order.MarkAsShipped();

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as shipped", id);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot mark order {OrderId} as shipped", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> MarkAsDeliveredAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        try
        {
            order.MarkAsDelivered();

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as delivered", id);

            return Result<OrderDto>.Success(MapToOrderDto(order));
        }
        catch (InvalidOperationDomainException ex)
        {
            _logger.LogWarning(ex, "Cannot mark order {OrderId} as delivered", id);
            return Result<OrderDto>.Failure(ex.Message, "DOMAIN_ERROR");
        }
    }

    public async Task<Result<OrderDto>> SetInternalNoteAsync(Guid id, string note, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found", "ORDER_NOT_FOUND");
        }

        order.SetInternalNote(note);

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Internal note set for order {OrderId}", id);

        return Result<OrderDto>.Success(MapToOrderDto(order));
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerCompanyName = order.Customer?.Title,
            Status = order.Status.ToString(),
            ApprovalStatus = order.ApprovalStatus.ToString(),
            Subtotal = order.Subtotal.Amount,
            TaxAmount = order.TaxAmount.Amount,
            DiscountAmount = order.DiscountAmount.Amount,
            ShippingCost = order.ShippingCost.Amount,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.Subtotal.Currency,
            LockedExchangeRate = order.LockedExchangeRate,
            ExchangeRateFrom = order.ExchangeRateFrom,
            ExchangeRateTo = order.ExchangeRateTo,
            ApprovedAt = order.ApprovedAt,
            ApprovedBy = order.ApprovedBy,
            RejectionReason = order.RejectionReason,
            ShippingStreet = order.ShippingAddress.Street,
            ShippingCity = order.ShippingAddress.City,
            ShippingState = order.ShippingAddress.State,
            ShippingCountry = order.ShippingAddress.Country,
            ShippingPostalCode = order.ShippingAddress.PostalCode,
            ShippingNote = order.ShippingNote,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            CustomerNote = order.CustomerNote,
            InternalNote = order.InternalNote,
            OrderItems = order.OrderItems.Select(MapToOrderItemDto).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt ?? order.CreatedAt
        };
    }

    private static OrderItemDto MapToOrderItemDto(OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductSKU = item.ProductSKU,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice.Amount,
            Currency = item.UnitPrice.Currency,
            Subtotal = item.Subtotal.Amount,
            TaxRate = item.TaxRate,
            TaxAmount = item.TaxAmount.Amount,
            DiscountAmount = item.DiscountAmount.Amount,
            TotalAmount = item.TotalAmount.Amount,
            SerialNumbers = item.SerialNumbers ?? new List<string>()
        };
    }
}
