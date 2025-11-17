using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service for credit management operations
/// </summary>
public interface ICreditManagementService
{
    bool ValidateCreditAvailability(Customer customer, Money orderAmount);
    void ReserveCredit(Customer customer, Money amount);
    void ReleaseCredit(Customer customer, Money amount);
    Money GetAvailableCredit(Customer customer);
    bool IsNearCreditLimit(Customer customer, decimal thresholdPercentage = 0.9m);
}
