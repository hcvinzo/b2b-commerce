using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Multiple addresses for a customer
/// </summary>
public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public string AddressTitle { get; private set; }
    public Address Address { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public Customer? Customer { get; set; }

    private CustomerAddress() // For EF Core
    {
        AddressTitle = string.Empty;
        Address = new Address("Street", "City", "State", "Country", "00000");
    }

    public CustomerAddress(Guid customerId, string addressTitle, Address address, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(addressTitle))
            throw new ArgumentException("Address title cannot be null or empty", nameof(addressTitle));

        CustomerId = customerId;
        AddressTitle = addressTitle;
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

    public void Update(string addressTitle, Address address)
    {
        if (string.IsNullOrWhiteSpace(addressTitle))
            throw new ArgumentException("Address title cannot be null or empty", nameof(addressTitle));

        AddressTitle = addressTitle;
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}
