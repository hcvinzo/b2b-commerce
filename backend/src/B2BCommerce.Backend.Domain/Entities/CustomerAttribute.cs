using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Stores additional customer information in a flexible EAV pattern with typed JSON data.
/// Used for B2B-specific information like shareholders, business partners, bank accounts, etc.
/// </summary>
public class CustomerAttribute : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public CustomerAttributeType AttributeType { get; private set; }
    public int DisplayOrder { get; private set; }
    public string JsonData { get; private set; } = string.Empty;

    private CustomerAttribute()
    {
    }

    /// <summary>
    /// Creates a new customer attribute
    /// </summary>
    public static CustomerAttribute Create(
        Guid customerId,
        CustomerAttributeType attributeType,
        string jsonData,
        int displayOrder = 0)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(jsonData))
        {
            throw new ArgumentException("JSON data cannot be empty", nameof(jsonData));
        }

        return new CustomerAttribute
        {
            CustomerId = customerId,
            AttributeType = attributeType,
            JsonData = jsonData,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the JSON data
    /// </summary>
    public void UpdateData(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            throw new ArgumentException("JSON data cannot be empty", nameof(jsonData));
        }

        JsonData = jsonData;
    }

    /// <summary>
    /// Updates the display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
