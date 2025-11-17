using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.DomainServices;

/// <summary>
/// Domain service implementation for pricing calculations
/// </summary>
public class PricingService : IPricingService
{
    public Money CalculatePrice(Product product, PriceTier tier, string targetCurrency)
    {
        var basePrice = product.GetPriceForTier(tier);

        // If target currency is same as product price currency, return as is
        if (basePrice.Currency == targetCurrency)
            return basePrice;

        // Note: Currency conversion should use current exchange rates from repository
        // This is a simplified version. Full implementation would query CurrencyRate entity
        return basePrice;
    }

    public Money ConvertCurrency(Money amount, string targetCurrency, decimal exchangeRate)
    {
        if (amount.Currency == targetCurrency)
            return amount;

        var convertedAmount = amount.Amount * exchangeRate;
        return new Money(convertedAmount, targetCurrency);
    }

    public Money CalculateOrderTotal(Order order)
    {
        // Total is already calculated in the Order entity
        return order.TotalAmount;
    }
}
