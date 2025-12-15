using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service implementation for order validation
/// </summary>
public class OrderValidationService : IOrderValidationService
{
    public bool ValidateOrderCanBeCreated(Customer customer, List<OrderItem> items)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        if (items is null || !items.Any())
            return false;

        // Customer must be approved
        if (!customer.IsApproved)
            return false;

        // Customer must be active
        if (!customer.IsActive)
            return false;

        return true;
    }

    public bool ValidateStockAvailability(Product product, int requestedQuantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        if (requestedQuantity <= 0)
            return false;

        return product.StockQuantity >= requestedQuantity;
    }

    public bool ValidateMinimumOrderQuantity(Product product, int requestedQuantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        return requestedQuantity >= product.MinimumOrderQuantity;
    }

    public bool ValidateCustomerApprovalStatus(Customer customer)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        return customer.IsApproved && customer.IsActive;
    }
}
