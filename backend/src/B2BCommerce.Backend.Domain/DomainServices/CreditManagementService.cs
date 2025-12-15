using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service implementation for credit management
/// </summary>
public class CreditManagementService : ICreditManagementService
{
    public bool ValidateCreditAvailability(Customer customer, Money orderAmount)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        if (orderAmount is null)
            throw new ArgumentNullException(nameof(orderAmount));

        return customer.HasSufficientCredit(orderAmount);
    }

    public void ReserveCredit(Customer customer, Money amount)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        if (amount is null)
            throw new ArgumentNullException(nameof(amount));

        if (!customer.HasSufficientCredit(amount))
            throw new InsufficientCreditException(
                customer.Id,
                amount.Amount,
                customer.GetAvailableCredit().Amount);

        customer.UseCredit(amount);
    }

    public void ReleaseCredit(Customer customer, Money amount)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        if (amount is null)
            throw new ArgumentNullException(nameof(amount));

        customer.ReleaseCredit(amount);
    }

    public Money GetAvailableCredit(Customer customer)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        return customer.GetAvailableCredit();
    }

    public bool IsNearCreditLimit(Customer customer, decimal thresholdPercentage = 0.9m)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        return customer.IsCreditNearLimit(thresholdPercentage);
    }
}
