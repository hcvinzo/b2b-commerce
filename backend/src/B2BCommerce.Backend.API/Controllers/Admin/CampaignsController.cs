using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Campaigns;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace B2BCommerce.Backend.API.Controllers.Admin;

/// <summary>
/// Controller for managing discount campaigns
/// </summary>
[ApiController]
[Route("api/admin/campaigns")]
[Authorize(Roles = "Admin")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    private string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(ClaimTypes.Email)?.Value ??
        "system";

    #region Campaign CRUD

    /// <summary>
    /// Get all campaigns with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CampaignListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] CampaignStatus? status = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] string sortBy = "priority",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var result = await _campaignService.GetAllAsync(
            search,
            status,
            startDateFrom,
            startDateTo,
            page,
            pageSize,
            sortBy,
            sortDirection,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get campaign by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get campaign by external ID
    /// </summary>
    [HttpGet("ext/{externalId}")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByExternalId(string externalId, CancellationToken cancellationToken)
    {
        var result = await _campaignService.GetByExternalIdAsync(externalId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new campaign
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto request, CancellationToken cancellationToken)
    {
        var result = await _campaignService.CreateAsync(request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing campaign
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto request, CancellationToken cancellationToken)
    {
        var result = await _campaignService.UpdateAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a campaign (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.DeleteAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    #endregion

    #region Campaign Status Actions

    /// <summary>
    /// Schedule a campaign (Draft → Scheduled)
    /// </summary>
    [HttpPost("{id:guid}/schedule")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.ScheduleAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Activate a campaign (Scheduled/Paused → Active)
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.ActivateAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Pause a campaign (Scheduled/Active → Paused)
    /// </summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.PauseAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Cancel a campaign
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.CancelAsync(id, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    #endregion

    #region Discount Rules

    /// <summary>
    /// Add a discount rule to a campaign
    /// </summary>
    [HttpPost("{id:guid}/rules")]
    [ProducesResponseType(typeof(DiscountRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDiscountRule(Guid id, [FromBody] CreateDiscountRuleDto request, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddDiscountRuleAsync(id, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id }, result.Data);
    }

    /// <summary>
    /// Update a discount rule
    /// </summary>
    [HttpPut("{id:guid}/rules/{ruleId:guid}")]
    [ProducesResponseType(typeof(DiscountRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDiscountRule(Guid id, Guid ruleId, [FromBody] UpdateDiscountRuleDto request, CancellationToken cancellationToken)
    {
        var result = await _campaignService.UpdateDiscountRuleAsync(id, ruleId, request, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Remove a discount rule from a campaign
    /// </summary>
    [HttpDelete("{id:guid}/rules/{ruleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDiscountRule(Guid id, Guid ruleId, CancellationToken cancellationToken)
    {
        var result = await _campaignService.RemoveDiscountRuleAsync(id, ruleId, GetCurrentUser(), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    #endregion

    #region Rule Targets

    /// <summary>
    /// Add products to a discount rule
    /// </summary>
    [HttpPost("{id:guid}/rules/{ruleId:guid}/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductsToRule(Guid id, Guid ruleId, [FromBody] List<Guid> productIds, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddProductsToRuleAsync(id, ruleId, productIds, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Products added successfully" });
    }

    /// <summary>
    /// Add categories to a discount rule
    /// </summary>
    [HttpPost("{id:guid}/rules/{ruleId:guid}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCategoriesToRule(Guid id, Guid ruleId, [FromBody] List<Guid> categoryIds, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddCategoriesToRuleAsync(id, ruleId, categoryIds, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Categories added successfully" });
    }

    /// <summary>
    /// Add brands to a discount rule
    /// </summary>
    [HttpPost("{id:guid}/rules/{ruleId:guid}/brands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBrandsToRule(Guid id, Guid ruleId, [FromBody] List<Guid> brandIds, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddBrandsToRuleAsync(id, ruleId, brandIds, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Brands added successfully" });
    }

    /// <summary>
    /// Add customers to a discount rule
    /// </summary>
    [HttpPost("{id:guid}/rules/{ruleId:guid}/customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCustomersToRule(Guid id, Guid ruleId, [FromBody] List<Guid> customerIds, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddCustomersToRuleAsync(id, ruleId, customerIds, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Customers added successfully" });
    }

    /// <summary>
    /// Add customer tiers to a discount rule
    /// </summary>
    [HttpPost("{id:guid}/rules/{ruleId:guid}/tiers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCustomerTiersToRule(Guid id, Guid ruleId, [FromBody] List<PriceTier> tiers, CancellationToken cancellationToken)
    {
        var result = await _campaignService.AddCustomerTiersToRuleAsync(id, ruleId, tiers, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { message = result.ErrorMessage });
            }
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Customer tiers added successfully" });
    }

    #endregion

    #region Usage Statistics

    /// <summary>
    /// Get usage statistics for a campaign
    /// </summary>
    [HttpGet("{id:guid}/usage-stats")]
    [ProducesResponseType(typeof(CampaignUsageStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsageStats(Guid id, CancellationToken cancellationToken)
    {
        var result = await _campaignService.GetUsageStatsAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    #endregion
}
