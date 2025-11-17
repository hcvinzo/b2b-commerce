using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service for pricing calculations
/// </summary>
public interface IPricingService
{
    Money CalculatePrice(Product product, PriceTier tier, string targetCurrency);
    Money ConvertCurrency(Money amount, string targetCurrency, decimal exchangeRate);
    Money CalculateOrderTotal(Order order);
}
