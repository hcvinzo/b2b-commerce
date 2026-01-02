using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Campaigns;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Service for calculating and applying campaign discounts
/// </summary>
public class CampaignDiscountService : ICampaignDiscountService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CampaignDiscountService> _logger;

    public CampaignDiscountService(
        ICampaignRepository campaignRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CampaignDiscountService> logger)
    {
        _campaignRepository = campaignRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DiscountCalculationResultDto?>> CalculateBestDiscountAsync(
        Guid productId,
        Guid customerId,
        PriceTier customerTier,
        Money unitPrice,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentTime = DateTime.UtcNow;

            // Get all active campaigns with their rules and targets
            var activeCampaigns = await _campaignRepository.GetActiveCampaignsAsync(currentTime, cancellationToken);

            if (!activeCampaigns.Any())
            {
                return Result<DiscountCalculationResultDto?>.Success(null);
            }

            // Get product details for targeting
            var product = await _productRepository.GetWithDetailsByIdAsync(productId, cancellationToken);

            if (product is null)
            {
                return Result<DiscountCalculationResultDto?>.Failure("Product not found");
            }

            // Get category ancestors for category targeting
            var categoryAncestorIds = new List<Guid>();
            var primaryCategory = product.ProductCategories.FirstOrDefault(pc => pc.IsPrimary)?.Category;
            if (primaryCategory is not null)
            {
                categoryAncestorIds = await GetCategoryAncestorIdsAsync(primaryCategory.Id, cancellationToken);
            }

            DiscountCalculationResultDto? bestDiscount = null;
            int bestCampaignPriority = int.MinValue;

            foreach (var campaign in activeCampaigns)
            {
                // Check if campaign has remaining budget
                if (!campaign.HasBudgetFor(Money.Zero(unitPrice.Currency)))
                {
                    continue;
                }

                // Check per-customer limits
                var (customerUsageCount, customerTotalDiscount) = await _campaignRepository.GetCustomerUsageAsync(
                    campaign.Id, customerId, cancellationToken);

                if (!campaign.HasCustomerBudgetFor(Money.Zero(unitPrice.Currency), customerUsageCount, new Money(customerTotalDiscount, unitPrice.Currency)))
                {
                    continue;
                }

                // Find applicable discount rules
                foreach (var rule in campaign.DiscountRules)
                {
                    // Check if rule applies to this product
                    if (!rule.AppliesToProduct(
                        productId,
                        primaryCategory?.Id,
                        categoryAncestorIds,
                        product.BrandId))
                    {
                        continue;
                    }

                    // Check if rule applies to this customer
                    if (!rule.AppliesToCustomer(customerId, customerTier))
                    {
                        continue;
                    }

                    // Calculate discount
                    var discountAmount = rule.CalculateDiscount(unitPrice, quantity, unitPrice.Currency);

                    if (discountAmount.Amount <= 0)
                    {
                        continue;
                    }

                    // Apply campaign budget constraints
                    if (campaign.TotalBudgetLimit is not null)
                    {
                        var remainingBudget = campaign.GetRemainingBudget();
                        if (remainingBudget is not null && discountAmount > remainingBudget)
                        {
                            discountAmount = remainingBudget;
                        }
                    }

                    // Apply per-customer budget constraints
                    if (campaign.PerCustomerBudgetLimit is not null)
                    {
                        var remainingCustomerBudget = campaign.PerCustomerBudgetLimit.Amount - customerTotalDiscount;
                        if (discountAmount.Amount > remainingCustomerBudget)
                        {
                            discountAmount = new Money(remainingCustomerBudget, discountAmount.Currency);
                        }
                    }

                    if (discountAmount.Amount <= 0)
                    {
                        continue;
                    }

                    var totalPrice = unitPrice.Amount * quantity;
                    var discountedTotal = totalPrice - discountAmount.Amount;

                    var result = new DiscountCalculationResultDto
                    {
                        CampaignId = campaign.Id,
                        CampaignName = campaign.Name,
                        DiscountRuleId = rule.Id,
                        DiscountType = rule.DiscountType,
                        DiscountValue = rule.DiscountValue,
                        DiscountAmount = discountAmount.Amount,
                        Currency = discountAmount.Currency,
                        OriginalUnitPrice = unitPrice.Amount,
                        DiscountedUnitPrice = discountedTotal / quantity,
                        OriginalTotalPrice = totalPrice,
                        DiscountedTotalPrice = discountedTotal,
                        Quantity = quantity
                    };

                    // Select best discount (highest savings first, then highest priority)
                    if (bestDiscount is null ||
                        result.DiscountAmount > bestDiscount.DiscountAmount ||
                        (result.DiscountAmount == bestDiscount.DiscountAmount && campaign.Priority > bestCampaignPriority))
                    {
                        bestDiscount = result;
                        bestCampaignPriority = campaign.Priority;
                    }
                }
            }

            return Result<DiscountCalculationResultDto?>.Success(bestDiscount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discount for product {ProductId}, customer {CustomerId}", productId, customerId);
            return Result<DiscountCalculationResultDto?>.Failure($"Error calculating discount: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<Guid, DiscountCalculationResultDto>>> CalculateDiscountsForItemsAsync(
        Guid customerId,
        PriceTier customerTier,
        IEnumerable<DiscountCalculationItemDto> items,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Guid, DiscountCalculationResultDto>();

        try
        {
            foreach (var item in items)
            {
                var unitPrice = new Money(item.UnitPrice, item.Currency);
                var discountResult = await CalculateBestDiscountAsync(
                    item.ProductId,
                    customerId,
                    customerTier,
                    unitPrice,
                    item.Quantity,
                    cancellationToken);

                if (discountResult.IsSuccess && discountResult.Data is not null)
                {
                    results[item.ProductId] = discountResult.Data;
                }
            }

            return Result<Dictionary<Guid, DiscountCalculationResultDto>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discounts for items");
            return Result<Dictionary<Guid, DiscountCalculationResultDto>>.Failure($"Error calculating discounts: {ex.Message}");
        }
    }

    public async Task<Result> RecordUsageAsync(
        Guid campaignId,
        Guid customerId,
        Guid orderId,
        Money discountAmount,
        Guid? orderItemId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);

            if (campaign is null)
            {
                return Result.Failure("Campaign not found");
            }

            // Create usage record
            var usage = CampaignUsage.Create(
                campaignId,
                customerId,
                orderId,
                discountAmount,
                orderItemId);

            await _campaignRepository.AddUsageAsync(usage, cancellationToken);

            // Update campaign totals
            campaign.RecordUsage(discountAmount);
            _campaignRepository.Update(campaign);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign usage recorded: Campaign {CampaignId}, Order {OrderId}, Amount {Amount}",
                campaignId, orderId, discountAmount.Amount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording campaign usage: Campaign {CampaignId}, Order {OrderId}", campaignId, orderId);
            return Result.Failure($"Error recording usage: {ex.Message}");
        }
    }

    public async Task<Result> ReverseUsageAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var usages = await _campaignRepository.GetUsagesByOrderAsync(orderId, cancellationToken);

            foreach (var usage in usages)
            {
                // Reverse the usage record
                usage.Reverse();

                // Update campaign totals
                usage.Campaign.ReverseUsage(usage.DiscountAmount);
                _campaignRepository.Update(usage.Campaign);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign usages reversed for order: {OrderId}, Count: {Count}", orderId, usages.Count());

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing campaign usage for order: {OrderId}", orderId);
            return Result.Failure($"Error reversing usage: {ex.Message}");
        }
    }

    private async Task<List<Guid>> GetCategoryAncestorIdsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var ancestors = new List<Guid>();
        var currentCategory = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        while (currentCategory?.ParentCategoryId is not null)
        {
            ancestors.Add(currentCategory.ParentCategoryId.Value);
            currentCategory = await _categoryRepository.GetByIdAsync(currentCategory.ParentCategoryId.Value, cancellationToken);
        }

        return ancestors;
    }
}
