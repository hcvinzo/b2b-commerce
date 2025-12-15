using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Order entity representing customer orders
/// </summary>
public class Order : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderApprovalStatus ApprovalStatus { get; private set; }

    // Financial
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money ShippingCost { get; private set; }
    public Money TotalAmount { get; private set; }

    // Exchange rate locked at approval
    public decimal? LockedExchangeRate { get; private set; }
    public string? ExchangeRateFrom { get; private set; }
    public string? ExchangeRateTo { get; private set; }

    // Approval
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    // Shipping
    public Address ShippingAddress { get; private set; }
    public string? ShippingNote { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }

    // Notes
    public string? CustomerNote { get; private set; }
    public string? InternalNote { get; private set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; private set; }
    public Payment? Payment { get; set; }
    public Shipment? Shipment { get; set; }

    private Order() // For EF Core
    {
        OrderNumber = string.Empty;
        Subtotal = Money.Zero("USD");
        TaxAmount = Money.Zero("USD");
        DiscountAmount = Money.Zero("USD");
        ShippingCost = Money.Zero("USD");
        TotalAmount = Money.Zero("USD");
        ShippingAddress = new Address("Street", "City", "State", "Country", "00000");
        OrderItems = new List<OrderItem>();
    }

    /// <summary>
    /// Creates a new Order instance
    /// </summary>
    public static Order Create(
        Guid customerId,
        Address shippingAddress,
        string currency,
        string? customerNote = null)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            ApprovalStatus = OrderApprovalStatus.PendingApproval,
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress)),
            CustomerNote = customerNote,
            Subtotal = Money.Zero(currency),
            TaxAmount = Money.Zero(currency),
            DiscountAmount = Money.Zero(currency),
            ShippingCost = Money.Zero(currency),
            TotalAmount = Money.Zero(currency),
            OrderItems = new List<OrderItem>()
        };

        return order;
    }

    [Obsolete("Use Order.Create() factory method instead")]
    public Order(
        Guid customerId,
        Address shippingAddress,
        string currency,
        string? customerNote = null)
    {
        OrderNumber = GenerateOrderNumber();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        ApprovalStatus = OrderApprovalStatus.PendingApproval;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        CustomerNote = customerNote;

        Subtotal = Money.Zero(currency);
        TaxAmount = Money.Zero(currency);
        DiscountAmount = Money.Zero(currency);
        ShippingCost = Money.Zero(currency);
        TotalAmount = Money.Zero(currency);

        OrderItems = new List<OrderItem>();
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    public void AddOrderItem(Guid productId, string productName, string productSKU,
        int quantity, Money unitPrice, decimal taxRate, Money? discount = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationDomainException($"Cannot add items to an order with status {Status}");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var orderItem = new OrderItem(Id, productId, productName, productSKU, quantity, unitPrice, taxRate, discount);
        OrderItems.Add(orderItem);

        RecalculateTotals();
    }

    public void RemoveOrderItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationDomainException($"Cannot remove items from an order with status {Status}");

        var item = OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item is not null)
        {
            OrderItems.Remove(item);
            RecalculateTotals();
        }
    }

    public void UpdateOrderItemQuantity(Guid orderItemId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationDomainException($"Cannot update items in an order with status {Status}");

        var item = OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item is not null)
        {
            item.UpdateQuantity(newQuantity);
            RecalculateTotals();
        }
    }

    public void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.Currency != Subtotal.Currency)
            throw new InvalidOperationDomainException($"Discount currency must match order currency {Subtotal.Currency}");

        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void SetShippingCost(Money shippingCost)
    {
        if (shippingCost.Currency != Subtotal.Currency)
            throw new InvalidOperationDomainException($"Shipping cost currency must match order currency {Subtotal.Currency}");

        ShippingCost = shippingCost;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = OrderItems.Aggregate(Money.Zero(Subtotal.Currency),
            (sum, item) => sum + item.Subtotal);

        TaxAmount = OrderItems.Aggregate(Money.Zero(TaxAmount.Currency),
            (sum, item) => sum + item.TaxAmount);

        TotalAmount = Subtotal + TaxAmount - DiscountAmount + ShippingCost;
    }

    public void Approve(string approvedBy, decimal? exchangeRate = null, string? exchangeRateFrom = null, string? exchangeRateTo = null)
    {
        if (ApprovalStatus == OrderApprovalStatus.Approved)
            throw new InvalidOperationDomainException("Order is already approved");

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approver information is required", nameof(approvedBy));

        if (!OrderItems.Any())
            throw new InvalidOperationDomainException("Cannot approve an order without items");

        ApprovalStatus = OrderApprovalStatus.Approved;
        Status = OrderStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;

        if (exchangeRate.HasValue)
        {
            LockedExchangeRate = exchangeRate.Value;
            ExchangeRateFrom = exchangeRateFrom;
            ExchangeRateTo = exchangeRateTo;
        }
    }

    public void Reject(string rejectedBy, string reason)
    {
        if (ApprovalStatus == OrderApprovalStatus.Rejected)
            throw new InvalidOperationDomainException("Order is already rejected");

        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejector information is required", nameof(rejectedBy));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        ApprovalStatus = OrderApprovalStatus.Rejected;
        Status = OrderStatus.Rejected;
        RejectionReason = reason;
        ApprovedBy = rejectedBy; // Using same field for tracking who rejected
        ApprovedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationDomainException("Cannot cancel a delivered order");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationDomainException("Order is already cancelled");

        Status = OrderStatus.Cancelled;
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Approved)
            throw new InvalidOperationDomainException("Only approved orders can be marked as processing");

        Status = OrderStatus.Processing;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationDomainException("Only processing orders can be marked as shipped");

        Status = OrderStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationDomainException("Only shipped orders can be marked as delivered");

        Status = OrderStatus.Delivered;
    }

    public void SetInternalNote(string note)
    {
        InternalNote = note;
    }

    public void UpdateEstimatedDeliveryDate(DateTime estimatedDate)
    {
        EstimatedDeliveryDate = estimatedDate;
    }
}
