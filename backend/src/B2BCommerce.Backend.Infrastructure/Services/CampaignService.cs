using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Campaigns;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Campaign service implementation
/// </summary>
public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(
        ICampaignRepository campaignRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<CampaignService> logger)
    {
        _campaignRepository = campaignRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CampaignListDto>>> GetAllAsync(
        string? search,
        CampaignStatus? status,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(1, pageNumber);

        var query = _campaignRepository.Query();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) ||
                                     (c.Description != null && c.Description.Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (startDateFrom.HasValue)
        {
            query = query.Where(c => c.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(c => c.StartDate <= startDateTo.Value);
        }

        // Apply sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "name" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "startdate" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.StartDate)
                : query.OrderBy(c => c.StartDate),
            "enddate" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.EndDate)
                : query.OrderBy(c => c.EndDate),
            "status" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.Status)
                : query.OrderBy(c => c.Status),
            "createdat" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.Priority)
                : query.OrderBy(c => c.Priority)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and includes
        var campaigns = await query
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = campaigns.Select(MapToListDto).ToList();

        var result = new PagedResult<CampaignListDto>(dtos, totalCount, pageNumber, pageSize);

        return Result<PagedResult<CampaignListDto>>.Success(result);
    }

    public async Task<Result<CampaignDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetWithRulesAndTargetsAsync(id, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignDto>.Failure("Campaign not found");
        }

        return Result<CampaignDto>.Success(MapToDto(campaign));
    }

    public async Task<Result<CampaignDto>> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByExternalIdAsync(externalId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignDto>.Failure("Campaign not found");
        }

        // Load full details
        campaign = await _campaignRepository.GetWithRulesAndTargetsAsync(campaign.Id, cancellationToken);

        return Result<CampaignDto>.Success(MapToDto(campaign!));
    }

    public async Task<Result<CampaignDto>> CreateAsync(CreateCampaignDto dto, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Money? totalBudgetLimit = dto.TotalBudgetLimitAmount.HasValue
                ? new Money(dto.TotalBudgetLimitAmount.Value, dto.Currency)
                : null;

            Money? perCustomerBudgetLimit = dto.PerCustomerBudgetLimitAmount.HasValue
                ? new Money(dto.PerCustomerBudgetLimitAmount.Value, dto.Currency)
                : null;

            Campaign campaign;

            if (!string.IsNullOrEmpty(dto.ExternalId))
            {
                campaign = Campaign.CreateFromExternal(
                    dto.ExternalId,
                    dto.Name,
                    dto.StartDate,
                    dto.EndDate,
                    dto.Description,
                    dto.Priority,
                    totalBudgetLimit,
                    dto.TotalUsageLimit,
                    perCustomerBudgetLimit,
                    dto.PerCustomerUsageLimit,
                    dto.ExternalCode,
                    currency: dto.Currency);
            }
            else
            {
                campaign = Campaign.Create(
                    dto.Name,
                    dto.StartDate,
                    dto.EndDate,
                    dto.Description,
                    dto.Priority,
                    totalBudgetLimit,
                    dto.TotalUsageLimit,
                    perCustomerBudgetLimit,
                    dto.PerCustomerUsageLimit,
                    dto.Currency);
            }

            campaign.CreatedBy = createdBy;

            await _campaignRepository.AddAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign created: {CampaignId} - {Name}", campaign.Id, campaign.Name);

            return await GetByIdAsync(campaign.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign: {Name}", dto.Name);
            return Result<CampaignDto>.Failure($"Error creating campaign: {ex.Message}");
        }
    }

    public async Task<Result<CampaignDto>> UpdateAsync(Guid id, UpdateCampaignDto dto, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result<CampaignDto>.Failure("Campaign not found");
            }

            var currency = dto.Currency ?? "TRY";

            Money? totalBudgetLimit = dto.TotalBudgetLimitAmount.HasValue
                ? new Money(dto.TotalBudgetLimitAmount.Value, currency)
                : null;

            Money? perCustomerBudgetLimit = dto.PerCustomerBudgetLimitAmount.HasValue
                ? new Money(dto.PerCustomerBudgetLimitAmount.Value, currency)
                : null;

            campaign.Update(
                dto.Name,
                dto.StartDate,
                dto.EndDate,
                dto.Description,
                dto.Priority,
                totalBudgetLimit,
                dto.TotalUsageLimit,
                perCustomerBudgetLimit,
                dto.PerCustomerUsageLimit);

            campaign.UpdatedBy = updatedBy;

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign updated: {CampaignId}", id);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign: {CampaignId}", id);
            return Result<CampaignDto>.Failure($"Error updating campaign: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result.Failure("Campaign not found");
            }

            campaign.MarkAsDeleted(deletedBy);

            _campaignRepository.Update(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign deleted: {CampaignId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign: {CampaignId}", id);
            return Result.Failure($"Error deleting campaign: {ex.Message}");
        }
    }

    public async Task<Result<CampaignDto>> ScheduleAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetWithRulesAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result<CampaignDto>.Failure("Campaign not found");
            }

            campaign.Schedule();
            campaign.UpdatedBy = updatedBy;

            // Note: Don't call Update() as the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign scheduled: {CampaignId}", id);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling campaign: {CampaignId}", id);
            return Result<CampaignDto>.Failure($"Error scheduling campaign: {ex.Message}");
        }
    }

    public async Task<Result<CampaignDto>> ActivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result<CampaignDto>.Failure("Campaign not found");
            }

            campaign.Activate();
            campaign.UpdatedBy = updatedBy;

            // Note: Don't call Update() as the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign activated: {CampaignId}", id);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating campaign: {CampaignId}", id);
            return Result<CampaignDto>.Failure($"Error activating campaign: {ex.Message}");
        }
    }

    public async Task<Result<CampaignDto>> PauseAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result<CampaignDto>.Failure("Campaign not found");
            }

            campaign.Pause();
            campaign.UpdatedBy = updatedBy;

            // Note: Don't call Update() as the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign paused: {CampaignId}", id);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign: {CampaignId}", id);
            return Result<CampaignDto>.Failure($"Error pausing campaign: {ex.Message}");
        }
    }

    public async Task<Result<CampaignDto>> CancelAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);

            if (campaign is null)
            {
                return Result<CampaignDto>.Failure("Campaign not found");
            }

            campaign.Cancel();
            campaign.UpdatedBy = updatedBy;

            // Note: Don't call Update() as the entity is already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Campaign cancelled: {CampaignId}", id);

            return await GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling campaign: {CampaignId}", id);
            return Result<CampaignDto>.Failure($"Error cancelling campaign: {ex.Message}");
        }
    }

    public async Task<Result<DiscountRuleDto>> AddDiscountRuleAsync(Guid campaignId, CreateDiscountRuleDto dto, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if campaign exists and is in valid status (without tracking to avoid concurrency issues)
            var campaignStatus = await _campaignRepository.GetStatusAsync(campaignId, cancellationToken);

            if (campaignStatus is null)
            {
                return Result<DiscountRuleDto>.Failure("Campaign not found");
            }

            if (campaignStatus != CampaignStatus.Draft)
            {
                return Result<DiscountRuleDto>.Failure($"Cannot add rules to campaign in {campaignStatus} status. Campaign must be in Draft status.");
            }

            // Create the rule and add it directly via DbContext to avoid tracking issues
            var rule = DiscountRule.Create(
                campaignId,
                dto.DiscountType,
                dto.DiscountValue,
                dto.ProductTargetType,
                dto.CustomerTargetType,
                dto.MaxDiscountAmount,
                dto.MinOrderAmount,
                dto.MinQuantity);

            rule.CreatedBy = createdBy;

            // Add targets based on type
            if (dto.ProductIds?.Any() == true && dto.ProductTargetType == ProductTargetType.SpecificProducts)
            {
                foreach (var productId in dto.ProductIds)
                {
                    rule.AddProduct(productId);
                }
            }

            if (dto.CategoryIds?.Any() == true && dto.ProductTargetType == ProductTargetType.Categories)
            {
                foreach (var categoryId in dto.CategoryIds)
                {
                    rule.AddCategory(categoryId);
                }
            }

            if (dto.BrandIds?.Any() == true && dto.ProductTargetType == ProductTargetType.Brands)
            {
                foreach (var brandId in dto.BrandIds)
                {
                    rule.AddBrand(brandId);
                }
            }

            if (dto.CustomerIds?.Any() == true && dto.CustomerTargetType == CustomerTargetType.SpecificCustomers)
            {
                foreach (var customerId in dto.CustomerIds)
                {
                    rule.AddCustomer(customerId);
                }
            }

            if (dto.CustomerTiers?.Any() == true && dto.CustomerTargetType == CustomerTargetType.CustomerTiers)
            {
                foreach (var tier in dto.CustomerTiers)
                {
                    rule.AddCustomerTier(tier);
                }
            }

            // Add the rule directly to the DbContext instead of through the Campaign collection
            // This avoids tracking issues with the Campaign's owned types
            await _campaignRepository.AddDiscountRuleAsync(rule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount rule added to campaign: {CampaignId} - {RuleId}", campaignId, rule.Id);

            var ruleWithTargets = await _campaignRepository.GetDiscountRuleWithTargetsAsync(rule.Id, cancellationToken);

            return Result<DiscountRuleDto>.Success(MapRuleToDto(ruleWithTargets!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding discount rule to campaign: {CampaignId}", campaignId);
            return Result<DiscountRuleDto>.Failure($"Error adding discount rule: {ex.Message}");
        }
    }

    public async Task<Result<DiscountRuleDto>> UpdateDiscountRuleAsync(Guid campaignId, Guid ruleId, UpdateDiscountRuleDto dto, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetWithRulesAsync(campaignId, cancellationToken);

            if (campaign is null)
            {
                return Result<DiscountRuleDto>.Failure("Campaign not found");
            }

            var rule = campaign.DiscountRules.FirstOrDefault(r => r.Id == ruleId);

            if (rule is null)
            {
                return Result<DiscountRuleDto>.Failure("Discount rule not found");
            }

            rule.Update(
                dto.DiscountType,
                dto.DiscountValue,
                dto.ProductTargetType,
                dto.CustomerTargetType,
                dto.MaxDiscountAmount,
                dto.MinOrderAmount,
                dto.MinQuantity);

            rule.UpdatedBy = updatedBy;

            // Note: Don't call Update() as the entities are already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount rule updated: {RuleId}", ruleId);

            var ruleWithTargets = await _campaignRepository.GetDiscountRuleWithTargetsAsync(ruleId, cancellationToken);

            return Result<DiscountRuleDto>.Success(MapRuleToDto(ruleWithTargets!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discount rule: {RuleId}", ruleId);
            return Result<DiscountRuleDto>.Failure($"Error updating discount rule: {ex.Message}");
        }
    }

    public async Task<Result> RemoveDiscountRuleAsync(Guid campaignId, Guid ruleId, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetWithRulesAsync(campaignId, cancellationToken);

            if (campaign is null)
            {
                return Result.Failure("Campaign not found");
            }

            var rule = campaign.DiscountRules.FirstOrDefault(r => r.Id == ruleId);

            if (rule is null)
            {
                return Result.Failure("Discount rule not found");
            }

            rule.MarkAsDeleted(deletedBy);

            // Note: Don't call Update() as the entities are already tracked by EF Core
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount rule removed from campaign: {CampaignId} - {RuleId}", campaignId, ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error removing discount rule: {ex.Message}");
        }
    }

    public async Task<Result> AddProductsToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate rule belongs to campaign without tracking
            var ruleCampaignId = await _campaignRepository.GetRuleCampaignIdAsync(ruleId, cancellationToken);

            if (ruleCampaignId is null || ruleCampaignId != campaignId)
            {
                return Result.Failure("Discount rule not found");
            }

            // Filter to valid products that exist
            var validProductIds = new List<Guid>();
            foreach (var productId in productIds)
            {
                var exists = await _productRepository.AnyAsync(p => p.Id == productId, cancellationToken);
                if (exists)
                {
                    validProductIds.Add(productId);
                }
            }

            // Create new junction entities
            var products = validProductIds.Select(productId => DiscountRuleProduct.Create(ruleId, productId)).ToList();

            // Replace all targets (removes existing, adds new)
            await _campaignRepository.ReplaceRuleProductsAsync(ruleId, products, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Products set for discount rule: {RuleId}", ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding products to discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error adding products: {ex.Message}");
        }
    }

    public async Task<Result> AddCategoriesToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> categoryIds, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate rule belongs to campaign without tracking
            var ruleCampaignId = await _campaignRepository.GetRuleCampaignIdAsync(ruleId, cancellationToken);

            if (ruleCampaignId is null || ruleCampaignId != campaignId)
            {
                return Result.Failure("Discount rule not found");
            }

            // Filter to valid categories that exist
            var validCategoryIds = new List<Guid>();
            foreach (var categoryId in categoryIds)
            {
                var exists = await _categoryRepository.AnyAsync(c => c.Id == categoryId, cancellationToken);
                if (exists)
                {
                    validCategoryIds.Add(categoryId);
                }
            }

            // Create new junction entities
            var categories = validCategoryIds.Select(categoryId => DiscountRuleCategory.Create(ruleId, categoryId)).ToList();

            // Replace all targets (removes existing, adds new)
            await _campaignRepository.ReplaceRuleCategoriesAsync(ruleId, categories, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Categories set for discount rule: {RuleId}", ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding categories to discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error adding categories: {ex.Message}");
        }
    }

    public async Task<Result> AddBrandsToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> brandIds, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate rule belongs to campaign without tracking
            var ruleCampaignId = await _campaignRepository.GetRuleCampaignIdAsync(ruleId, cancellationToken);

            if (ruleCampaignId is null || ruleCampaignId != campaignId)
            {
                return Result.Failure("Discount rule not found");
            }

            // Filter to valid brands that exist
            var validBrandIds = new List<Guid>();
            foreach (var brandId in brandIds)
            {
                var exists = await _brandRepository.AnyAsync(b => b.Id == brandId, cancellationToken);
                if (exists)
                {
                    validBrandIds.Add(brandId);
                }
            }

            // Create new junction entities
            var brands = validBrandIds.Select(brandId => DiscountRuleBrand.Create(ruleId, brandId)).ToList();

            // Replace all targets (removes existing, adds new)
            await _campaignRepository.ReplaceRuleBrandsAsync(ruleId, brands, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Brands set for discount rule: {RuleId}", ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding brands to discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error adding brands: {ex.Message}");
        }
    }

    public async Task<Result> AddCustomersToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> customerIds, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate rule belongs to campaign without tracking
            var ruleCampaignId = await _campaignRepository.GetRuleCampaignIdAsync(ruleId, cancellationToken);

            if (ruleCampaignId is null || ruleCampaignId != campaignId)
            {
                return Result.Failure("Discount rule not found");
            }

            // Filter to valid customers that exist
            var validCustomerIds = new List<Guid>();
            foreach (var customerId in customerIds)
            {
                var exists = await _customerRepository.AnyAsync(c => c.Id == customerId, cancellationToken);
                if (exists)
                {
                    validCustomerIds.Add(customerId);
                }
            }

            // Create new junction entities
            var customers = validCustomerIds.Select(customerId => DiscountRuleCustomer.Create(ruleId, customerId)).ToList();

            // Replace all targets (removes existing, adds new)
            await _campaignRepository.ReplaceRuleCustomersAsync(ruleId, customers, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customers set for discount rule: {RuleId}", ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customers to discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error adding customers: {ex.Message}");
        }
    }

    public async Task<Result> AddCustomerTiersToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<PriceTier> tiers, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate rule belongs to campaign without tracking
            var ruleCampaignId = await _campaignRepository.GetRuleCampaignIdAsync(ruleId, cancellationToken);

            if (ruleCampaignId is null || ruleCampaignId != campaignId)
            {
                return Result.Failure("Discount rule not found");
            }

            // Create new junction entities
            var tierEntities = tiers.Select(tier => DiscountRuleCustomerTier.Create(ruleId, tier)).ToList();

            // Replace all targets (removes existing, adds new)
            await _campaignRepository.ReplaceRuleCustomerTiersAsync(ruleId, tierEntities, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer tiers set for discount rule: {RuleId}", ruleId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customer tiers to discount rule: {RuleId}", ruleId);
            return Result.Failure($"Error adding customer tiers: {ex.Message}");
        }
    }

    public async Task<Result<CampaignUsageStatsDto>> GetUsageStatsAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);

        if (campaign is null)
        {
            return Result<CampaignUsageStatsDto>.Failure("Campaign not found");
        }

        var stats = new CampaignUsageStatsDto
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            TotalUsageCount = campaign.TotalUsageCount,
            TotalDiscountUsedAmount = campaign.TotalDiscountUsed.Amount,
            Currency = campaign.TotalDiscountUsed.Currency,
            TotalBudgetLimit = campaign.TotalBudgetLimit?.Amount,
            TotalUsageLimit = campaign.TotalUsageLimit,
            RemainingBudget = campaign.GetRemainingBudget()?.Amount,
            RemainingUsageCount = campaign.TotalUsageLimit.HasValue
                ? campaign.TotalUsageLimit.Value - campaign.TotalUsageCount
                : null
        };

        return Result<CampaignUsageStatsDto>.Success(stats);
    }

    private static CampaignListDto MapToListDto(Campaign campaign)
    {
        return new CampaignListDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Status = campaign.Status,
            Priority = campaign.Priority,
            TotalBudgetLimitAmount = campaign.TotalBudgetLimit?.Amount,
            TotalBudgetLimitCurrency = campaign.TotalBudgetLimit?.Currency,
            TotalDiscountUsedAmount = campaign.TotalDiscountUsed.Amount,
            TotalDiscountUsedCurrency = campaign.TotalDiscountUsed.Currency,
            TotalUsageCount = campaign.TotalUsageCount,
            TotalUsageLimit = campaign.TotalUsageLimit,
            DiscountRuleCount = campaign.DiscountRules.Count,
            ExternalCode = campaign.ExternalCode,
            ExternalId = campaign.ExternalId,
            LastSyncedAt = campaign.LastSyncedAt,
            CreatedAt = campaign.CreatedAt
        };
    }

    private static CampaignDto MapToDto(Campaign campaign)
    {
        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Status = campaign.Status,
            Priority = campaign.Priority,
            TotalBudgetLimitAmount = campaign.TotalBudgetLimit?.Amount,
            TotalBudgetLimitCurrency = campaign.TotalBudgetLimit?.Currency,
            TotalUsageLimit = campaign.TotalUsageLimit,
            PerCustomerBudgetLimitAmount = campaign.PerCustomerBudgetLimit?.Amount,
            PerCustomerBudgetLimitCurrency = campaign.PerCustomerBudgetLimit?.Currency,
            PerCustomerUsageLimit = campaign.PerCustomerUsageLimit,
            TotalDiscountUsedAmount = campaign.TotalDiscountUsed.Amount,
            TotalDiscountUsedCurrency = campaign.TotalDiscountUsed.Currency,
            TotalUsageCount = campaign.TotalUsageCount,
            DiscountRules = campaign.DiscountRules.Select(MapRuleToDto).ToList(),
            ExternalCode = campaign.ExternalCode,
            ExternalId = campaign.ExternalId,
            LastSyncedAt = campaign.LastSyncedAt,
            CreatedAt = campaign.CreatedAt,
            CreatedBy = campaign.CreatedBy,
            UpdatedAt = campaign.UpdatedAt,
            UpdatedBy = campaign.UpdatedBy
        };
    }

    private static DiscountRuleDto MapRuleToDto(DiscountRule rule)
    {
        return new DiscountRuleDto
        {
            Id = rule.Id,
            CampaignId = rule.CampaignId,
            DiscountType = rule.DiscountType,
            DiscountValue = rule.DiscountValue,
            MaxDiscountAmount = rule.MaxDiscountAmount,
            ProductTargetType = rule.ProductTargetType,
            CustomerTargetType = rule.CustomerTargetType,
            MinOrderAmount = rule.MinOrderAmount,
            MinQuantity = rule.MinQuantity,
            Products = rule.Products.Select(p => new DiscountRuleProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.Product?.Name,
                ProductSku = p.Product?.SKU
            }).ToList(),
            Categories = rule.Categories.Select(c => new DiscountRuleCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.Category?.Name
            }).ToList(),
            Brands = rule.Brands.Select(b => new DiscountRuleBrandDto
            {
                BrandId = b.BrandId,
                BrandName = b.Brand?.Name
            }).ToList(),
            Customers = rule.Customers.Select(c => new DiscountRuleCustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerTitle = c.Customer?.Title
            }).ToList(),
            CustomerTiers = rule.CustomerTiers.Select(t => new DiscountRuleCustomerTierDto
            {
                PriceTier = t.PriceTier
            }).ToList(),
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
