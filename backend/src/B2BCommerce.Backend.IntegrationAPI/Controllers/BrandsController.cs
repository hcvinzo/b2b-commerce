using B2BCommerce.Backend.Application.Features.Brands.Commands.UpsertBrand;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Brands API endpoints for external integrations (LOGO ERP).
/// All IDs are ExternalIds (string). Internal Guids are never exposed.
/// </summary>
public class BrandsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<BrandsController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public BrandsController(
        IMediator mediator,
        ILogger<BrandsController> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all brands with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "brands:read")]
    [ProducesResponseType(typeof(Models.PagedApiResponse<BrandListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBrands([FromQuery] BrandFilterDto filter)
    {
        var brands = await GetBrandsWithFilters(
            filter.Search,
            filter.IsActive,
            filter.PageNumber,
            filter.PageSize,
            filter.SortBy,
            filter.SortDirection);

        return PagedResponse(
            brands.Items.Select(MapToBrandListDto).ToList(),
            brands.PageNumber,
            brands.PageSize,
            brands.TotalCount);
    }

    /// <summary>
    /// Get a brand by ID (ExternalId)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "brands:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrand(string id)
    {
        var brand = await _unitOfWork.Brands.GetByExternalIdAsync(id);

        if (brand == null)
        {
            return NotFoundResponse($"Brand with ID '{id}' not found");
        }

        return OkResponse(MapToBrandDto(brand));
    }

    /// <summary>
    /// Upsert brand. If brand with given Id (ExternalId) or Name exists, it is updated; otherwise, a new brand is created.
    /// Id is required for creating new brands.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "brands:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertBrand([FromBody] BrandSyncRequest request)
    {
        var command = new UpsertBrandCommand
        {
            ExternalId = request.Id,
            ExternalCode = request.Code,
            Name = request.Name,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
            IsActive = request.IsActive,
            ModifiedBy = GetClientName()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "EXTERNAL_ID_REQUIRED" => BadRequestResponse(result.ErrorMessage ?? "Id is required for new brands", result.ErrorCode),
                _ => BadRequestResponse(result.ErrorMessage ?? "Failed to sync brand", result.ErrorCode)
            };
        }

        _logger.LogInformation(
            "Brand {Id}/{Name} synced by API client {ClientName}",
            request.Id,
            request.Name,
            GetClientName());

        return OkResponse(MapFromApplicationDto(result.Data!));
    }

    /// <summary>
    /// Delete a brand by ID (ExternalId) - soft delete
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "brands:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrand(string id)
    {
        var brand = await _unitOfWork.Brands.GetByExternalIdAsync(id);

        if (brand == null)
        {
            return NotFoundResponse($"Brand with ID '{id}' not found");
        }

        brand.MarkAsDeleted(GetClientName());
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Brand with ID {Id} deleted by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse<object?>(null, "Brand deleted successfully");
    }

    #region Private Methods

    private async Task<(List<Domain.Entities.Brand> Items, int PageNumber, int PageSize, int TotalCount)> GetBrandsWithFilters(
        string? search,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection)
    {
        // Ensure valid pagination
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Get all brands
        var allBrands = await _unitOfWork.Brands.GetAllAsync();
        var query = allBrands.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(lowerSearch) ||
                                     b.Description.ToLower().Contains(lowerSearch));
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        // Apply sorting
        query = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(b => b.Name),
            ("name", _) => query.OrderBy(b => b.Name),
            ("createdat", "desc") => query.OrderByDescending(b => b.CreatedAt),
            ("createdat", _) => query.OrderBy(b => b.CreatedAt),
            ("updatedat", "desc") => query.OrderByDescending(b => b.UpdatedAt),
            ("updatedat", _) => query.OrderBy(b => b.UpdatedAt),
            _ => query.OrderBy(b => b.Name)
        };

        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, pageNumber, pageSize, totalCount);
    }

    private static BrandDto MapToBrandDto(Domain.Entities.Brand brand)
    {
        return new BrandDto
        {
            Id = brand.ExternalId ?? string.Empty,
            Code = brand.ExternalCode,
            Name = brand.Name,
            Description = brand.Description,
            LogoUrl = brand.LogoUrl,
            WebsiteUrl = brand.WebsiteUrl,
            IsActive = brand.IsActive,
            LastSyncedAt = brand.LastSyncedAt,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }

    private static BrandListDto MapToBrandListDto(Domain.Entities.Brand brand)
    {
        return new BrandListDto
        {
            Id = brand.ExternalId ?? string.Empty,
            Code = brand.ExternalCode,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            IsActive = brand.IsActive,
            ProductCount = brand.Products?.Count ?? 0,
            LastSyncedAt = brand.LastSyncedAt
        };
    }

    private static BrandDto MapFromApplicationDto(Application.DTOs.Brands.BrandDto source)
    {
        return new BrandDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.ExternalCode,
            Name = source.Name,
            Description = source.Description,
            LogoUrl = source.LogoUrl,
            WebsiteUrl = source.WebsiteUrl,
            IsActive = source.IsActive,
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    #endregion
}
