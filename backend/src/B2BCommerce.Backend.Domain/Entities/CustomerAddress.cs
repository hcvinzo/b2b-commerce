using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Customer address entity supporting multiple addresses per customer
/// </summary>
public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; }
    public CustomerAddressType AddressType { get; private set; }
    public Address Address { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public Customer? Customer { get; set; }

    private CustomerAddress() // For EF Core
    {
        Title = string.Empty;
        Address = new Address("Street", "City", "State", "Country", "00000");
    }

    /// <summary>
    /// Creates a new customer address
    /// </summary>
    public static CustomerAddress Create(
        Guid customerId,
        string title,
        CustomerAddressType addressType,
        Address address,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Address title cannot be null or empty", nameof(title));
        }

        return new CustomerAddress
        {
            CustomerId = customerId,
            Title = title,
            AddressType = addressType,
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            IsDefault = isDefault,
            IsActive = true
        };
    }

    [Obsolete("Use CustomerAddress.Create() factory method instead")]
    public CustomerAddress(Guid customerId, string addressTitle, Address address, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(addressTitle))
        {
            throw new ArgumentException("Address title cannot be null or empty", nameof(addressTitle));
        }

        CustomerId = customerId;
        Title = addressTitle;
        AddressType = CustomerAddressType.Shipping;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        IsDefault = isDefault;
        IsActive = true;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
    }

    public void Update(string title, Address address)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Address title cannot be null or empty", nameof(title));
        }

        Title = title;
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void UpdateAddressType(CustomerAddressType addressType)
    {
        AddressType = addressType;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
