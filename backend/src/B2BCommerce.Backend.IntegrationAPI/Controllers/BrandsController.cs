using B2BCommerce.Backend.Application.Features.Brands.Commands.UpsertBrand;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Marka API uç noktaları - harici entegrasyonlar için (LOGO ERP).
/// Tüm ID'ler ExternalId (string) formatındadır. Dahili Guid'ler asla dışarıya açılmaz.
/// </summary>
/// <remarks>
/// Bu API, ERP sistemlerinden marka senkronizasyonu için kullanılır.
/// Markalar ürünlerle ilişkilendirilebilir.
/// </remarks>
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
    /// Tüm markaları filtreleme ve sayfalama ile getirir
    /// </summary>
    /// <param name="filter">Filtreleme parametreleri (arama, aktiflik durumu, sayfalama)</param>
    /// <returns>Sayfalanmış marka listesi</returns>
    /// <response code="200">Markalar başarıyla getirildi</response>
    /// <response code="400">Geçersiz filtre parametresi</response>
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
    /// Belirtilen ID'ye (ExternalId) sahip markayı getirir
    /// </summary>
    /// <param name="id">Markanın harici ID'si (ERP sisteminden gelen ID)</param>
    /// <returns>Marka detayları</returns>
    /// <response code="200">Marka başarıyla getirildi</response>
    /// <response code="404">Marka bulunamadı</response>
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
    /// Marka oluşturur veya günceller (Upsert). Verilen Id (ExternalId) veya Ad ile marka varsa güncellenir, yoksa yeni oluşturulur.
    /// </summary>
    /// <param name="request">Marka bilgileri</param>
    /// <returns>Oluşturulan veya güncellenen marka</returns>
    /// <remarks>
    /// Yeni marka oluşturmak için Id (ExternalId) zorunludur.
    /// Marka adı benzersiz olmalıdır.
    /// </remarks>
    /// <response code="200">Marka başarıyla oluşturuldu/güncellendi</response>
    /// <response code="400">Geçersiz istek (Id eksik)</response>
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
            IsActive = request.IsActive
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
    /// Belirtilen ID'ye (ExternalId) sahip markayı siler (soft delete)
    /// </summary>
    /// <param name="id">Silinecek markanın harici ID'si</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <remarks>
    /// Soft delete işlemi yapılır, kayıt tamamen silinmez.
    /// </remarks>
    /// <response code="200">Marka başarıyla silindi</response>
    /// <response code="404">Marka bulunamadı</response>
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
