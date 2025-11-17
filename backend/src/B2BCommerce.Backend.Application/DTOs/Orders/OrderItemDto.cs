namespace B2BCommerce.Backend.Application.DTOs.Orders;

/// <summary>
/// Order item data transfer object for output
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Order item identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU
    /// </summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price amount
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal amount (before tax and discount)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Tax rate (e.g., 0.18 for 18%)
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total amount (including tax and discount)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Serial numbers (for serialized products)
    /// </summary>
    public List<string> SerialNumbers { get; set; } = new();
}
