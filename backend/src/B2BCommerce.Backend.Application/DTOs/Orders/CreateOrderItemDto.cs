namespace B2BCommerce.Backend.Application.DTOs.Orders;

/// <summary>
/// Data transfer object for creating an order item
/// </summary>
public class CreateOrderItemDto
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity to order
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional discount amount for this item
    /// </summary>
    public decimal? DiscountAmount { get; set; }
}
