using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service for order validation
/// </summary>
public interface IOrderValidationService
{
    bool ValidateOrderCanBeCreated(Customer customer, List<OrderItem> items);
    bool ValidateStockAvailability(Product product, int requestedQuantity);
    bool ValidateMinimumOrderQuantity(Product product, int requestedQuantity);
    bool ValidateCustomerApprovalStatus(Customer customer);
}
