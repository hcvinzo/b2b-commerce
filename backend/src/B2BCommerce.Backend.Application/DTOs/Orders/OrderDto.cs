namespace B2BCommerce.Backend.Application.DTOs.Orders;

/// <summary>
/// Order data transfer object for output
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Order identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Order number
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Customer company name
    /// </summary>
    public string? CustomerCompanyName { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Approval status
    /// </summary>
    public string ApprovalStatus { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal amount
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Locked exchange rate
    /// </summary>
    public decimal? LockedExchangeRate { get; set; }

    /// <summary>
    /// Exchange rate from currency
    /// </summary>
    public string? ExchangeRateFrom { get; set; }

    /// <summary>
    /// Exchange rate to currency
    /// </summary>
    public string? ExchangeRateTo { get; set; }

    /// <summary>
    /// Date order was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Who approved the order
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Reason for rejection (if rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Shipping address street
    /// </summary>
    public string ShippingStreet { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address city
    /// </summary>
    public string ShippingCity { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address state
    /// </summary>
    public string ShippingState { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address country
    /// </summary>
    public string ShippingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address postal code
    /// </summary>
    public string ShippingPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Shipping note
    /// </summary>
    public string? ShippingNote { get; set; }

    /// <summary>
    /// Estimated delivery date
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>
    /// Customer note
    /// </summary>
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Internal note
    /// </summary>
    public string? InternalNote { get; set; }

    /// <summary>
    /// Order items
    /// </summary>
    public List<OrderItemDto> OrderItems { get; set; } = new();

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
