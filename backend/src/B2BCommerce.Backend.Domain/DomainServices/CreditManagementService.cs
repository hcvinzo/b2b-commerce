using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service implementation for credit management.
/// Note: Credit management has moved to dynamic CustomerAttributes (EAV pattern).
/// This service now provides placeholder implementations that should be replaced
/// with Application layer services that query CustomerAttribute values.
/// </summary>
public class CreditManagementService : ICreditManagementService
{
    /// <summary>
    /// Validates credit availability for a customer.
    /// Credit limit and used credit are now stored as CustomerAttributes.
    /// This method currently always returns true as a placeholder.
    /// </summary>
    public bool ValidateCreditAvailability(Customer customer, Money orderAmount)
    {
        if (customer is null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        if (orderAmount is null)
        {
            throw new ArgumentNullException(nameof(orderAmount));
        }

        // Credit management has moved to dynamic attributes.
        // Application layer should query CustomerAttribute for credit_limit and used_credit.
        // For now, return true to allow orders (credit validation bypassed).
        return customer.Status == CustomerStatus.Active;
    }

    /// <summary>
    /// Reserves credit for an order.
    /// Credit is now managed via CustomerAttributes.
    /// </summary>
    public void ReserveCredit(Customer customer, Money amount)
    {
        if (customer is null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        if (amount is null)
        {
            throw new ArgumentNullException(nameof(amount));
        }

        // Credit management has moved to dynamic attributes.
        // Application layer should update used_credit CustomerAttribute.
        // This is now a no-op at the domain level.
    }

    /// <summary>
    /// Releases reserved credit for an order.
    /// Credit is now managed via CustomerAttributes.
    /// </summary>
    public void ReleaseCredit(Customer customer, Money amount)
    {
        if (customer is null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        if (amount is null)
        {
            throw new ArgumentNullException(nameof(amount));
        }

        // Credit management has moved to dynamic attributes.
        // Application layer should update used_credit CustomerAttribute.
        // This is now a no-op at the domain level.
    }

    /// <summary>
    /// Gets available credit for a customer.
    /// Credit is now managed via CustomerAttributes.
    /// </summary>
    public Money GetAvailableCredit(Customer customer)
    {
        if (customer is null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        // Credit management has moved to dynamic attributes.
        // Application layer should query credit_limit and used_credit CustomerAttributes.
        // Returns zero as placeholder - actual implementation should be in Application layer.
        return new Money(0m, "TRY");
    }

    /// <summary>
    /// Checks if customer is near their credit limit.
    /// Credit is now managed via CustomerAttributes.
    /// </summary>
    public bool IsNearCreditLimit(Customer customer, decimal thresholdPercentage = 0.9m)
    {
        if (customer is null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        // Credit management has moved to dynamic attributes.
        // Application layer should query credit_limit and used_credit CustomerAttributes.
        // Returns false as placeholder - actual implementation should be in Application layer.
        return false;
    }
}
