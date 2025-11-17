using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Order item entity representing individual products in an order
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSKU { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Subtotal { get; private set; }
    public decimal TaxRate { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }

    // Serial numbers for serialized products
    public List<string> SerialNumbers { get; private set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }

    private OrderItem() // For EF Core
    {
        ProductName = string.Empty;
        ProductSKU = string.Empty;
        UnitPrice = Money.Zero("USD");
        Subtotal = Money.Zero("USD");
        TaxAmount = Money.Zero("USD");
        DiscountAmount = Money.Zero("USD");
        TotalAmount = Money.Zero("USD");
        SerialNumbers = new List<string>();
    }

    public OrderItem(
        Guid orderId,
        Guid productId,
        string productName,
        string productSKU,
        int quantity,
        Money unitPrice,
        decimal taxRate,
        Money? discountAmount = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

        if (string.IsNullOrWhiteSpace(productSKU))
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(productSKU));

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSKU = productSKU;
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        TaxRate = taxRate;
        DiscountAmount = discountAmount ?? Money.Zero(unitPrice.Currency);
        SerialNumbers = new List<string>();

        CalculateAmounts();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        CalculateAmounts();
    }

    public void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.Currency != UnitPrice.Currency)
            throw new ArgumentException($"Discount currency must match unit price currency {UnitPrice.Currency}");

        DiscountAmount = discountAmount;
        CalculateAmounts();
    }

    public void AddSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be null or empty", nameof(serialNumber));

        if (!SerialNumbers.Contains(serialNumber))
            SerialNumbers.Add(serialNumber);
    }

    private void CalculateAmounts()
    {
        Subtotal = UnitPrice * Quantity;
        var discountedAmount = Subtotal - DiscountAmount;
        TaxAmount = discountedAmount * TaxRate;
        TotalAmount = discountedAmount + TaxAmount;
    }
}
