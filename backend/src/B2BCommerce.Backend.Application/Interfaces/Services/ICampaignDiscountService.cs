using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Campaigns;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service for calculating and applying campaign discounts
/// </summary>
public interface ICampaignDiscountService
{
    /// <summary>
    /// Calculates the best discount for a product and customer
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerTier">Customer's price tier</param>
    /// <param name="unitPrice">Product unit price</param>
    /// <param name="quantity">Quantity being purchased</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best discount calculation result or null if no discount applies</returns>
    Task<Result<DiscountCalculationResultDto?>> CalculateBestDiscountAsync(
        Guid productId,
        Guid customerId,
        PriceTier customerTier,
        Money unitPrice,
        int quantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates discounts for multiple products in a single call
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerTier">Customer's price tier</param>
    /// <param name="items">Items to calculate discounts for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of product ID to discount calculation result</returns>
    Task<Result<Dictionary<Guid, DiscountCalculationResultDto>>> CalculateDiscountsForItemsAsync(
        Guid customerId,
        PriceTier customerTier,
        IEnumerable<DiscountCalculationItemDto> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records usage of a campaign discount
    /// </summary>
    /// <param name="campaignId">Campaign ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="discountAmount">Discount amount</param>
    /// <param name="orderItemId">Optional order item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> RecordUsageAsync(
        Guid campaignId,
        Guid customerId,
        Guid orderId,
        Money discountAmount,
        Guid? orderItemId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses all discount usages for an order (e.g., when order is cancelled)
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> ReverseUsageAsync(Guid orderId, CancellationToken cancellationToken = default);
}
