namespace B2BCommerce.Backend.Application.DTOs.Orders;

/// <summary>
/// Data transfer object for creating a new order
/// </summary>
public class CreateOrderDto
{
    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

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
    /// Currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Customer note
    /// </summary>
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Shipping note
    /// </summary>
    public string? ShippingNote { get; set; }

    /// <summary>
    /// Order items
    /// </summary>
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();

    /// <summary>
    /// Optional discount amount for the entire order
    /// </summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// Optional shipping cost
    /// </summary>
    public decimal? ShippingCost { get; set; }
}
