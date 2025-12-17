using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.DeleteProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpsertProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Ürün Tipi API uç noktaları - harici entegrasyonlar için.
/// Tüm işlemler birincil tanımlayıcı olarak ExternalId kullanır.
/// </summary>
/// <remarks>
/// Bu API, ürün tiplerinin ve bunlara atanmış özelliklerin yönetimi için kullanılır.
/// Ürün tipleri, benzer özelliklere sahip ürünleri gruplamak için kullanılır.
///
/// **Örnek kullanım senaryoları:**
/// - "Dizüstü Bilgisayar" tipi: RAM, işlemci, ekran boyutu özellikleri
/// - "Yazıcı" tipi: Baskı hızı, kağıt kapasitesi, bağlantı tipleri özellikleri
/// - "Monitör" tipi: Ekran boyutu, çözünürlük, panel tipi özellikleri
///
/// Her ürün tipine atanan özellikler, o tipe ait ürünlerde doldurulması gereken/beklenen alanları belirler.
/// </remarks>
[Route("api/v1/producttypes")]
public class ProductTypesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductTypesController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductTypesController(
        IMediator mediator,
        ILogger<ProductTypesController> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Read Operations

    /// <summary>
    /// Tüm ürün tiplerini isteğe bağlı filtreleme ile getirir
    /// </summary>
    /// <param name="filter">Filtreleme parametreleri</param>
    /// <returns>Ürün tipleri listesi</returns>
    /// <remarks>
    /// ERP sistemlerinin ürün tipi tanımlarını senkronize etmesi için kullanılır.
    /// Varsayılan olarak tüm ürün tipleri döner; IsActive filtresi ile aktif/pasif filtrelenebilir.
    /// </remarks>
    /// <response code="200">Ürün tipleri başarıyla getirildi</response>
    [HttpGet]
    [Authorize(Policy = "product-types:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<ProductTypeListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductTypes([FromQuery] ProductTypeFilterDto filter)
    {
        var query = new GetProductTypesQuery(filter.IsActive);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get product types", result.ErrorCode);
        }

        var dtos = result.Data!.Select(MapToProductTypeListDto).ToList();

        return OkResponse(dtos);
    }

    /// <summary>
    /// Belirtilen ID'ye (ExternalId) sahip ürün tipini özellik detaylarıyla getirir
    /// </summary>
    /// <param name="id">Ürün tipinin harici ID'si</param>
    /// <returns>Özellik şeması dahil ürün tipi detayları</returns>
    /// <remarks>
    /// Ürün tipine atanmış tüm özellikleri ve bu özelliklerin önceden tanımlı değerlerini döner.
    /// Bu endpoint, ürün oluşturma/düzenleme formları için gerekli özellik şemasını almak için kullanılır.
    /// </remarks>
    /// <response code="200">Ürün tipi başarıyla getirildi</response>
    /// <response code="404">Ürün tipi bulunamadı</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "product-types:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductType(string id)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesByExternalIdAsync(id);

        if (productType == null)
        {
            return NotFoundResponse($"Product type with ID '{id}' not found");
        }

        return OkResponse(MapEntityToDto(productType));
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Ürün tipini oluşturur veya günceller (Upsert)
    /// </summary>
    /// <param name="request">Ürün tipi verileri</param>
    /// <returns>Oluşturulan veya güncellenen ürün tipi</returns>
    /// <remarks>
    /// Belirtilen ID (ExternalId) ile ürün tipi varsa güncellenir, yoksa yeni ürün tipi oluşturulur.
    ///
    /// **Önemli Notlar:**
    /// - Id (ExternalId) yeni ürün tipi oluşturmak için zorunludur
    /// - Code alanı sistem genelinde benzersiz olmalıdır
    /// - Attributes dizisi ile özellikleri atayabilirsiniz (ExternalId veya Code ile)
    /// - Attributes gönderildiğinde, mevcut özellikler tamamen değiştirilir (full replacement)
    ///
    /// **Örnek İstek:**
    /// ```json
    /// {
    ///   "id": "TYPE-LAPTOP",
    ///   "code": "laptop",
    ///   "name": "Dizüstü Bilgisayar",
    ///   "isActive": true,
    ///   "attributes": [
    ///     { "attributeId": "ATTR-RAM", "isRequired": true, "displayOrder": 1 },
    ///     { "attributeCode": "ekran_boyutu", "isRequired": false, "displayOrder": 2 }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Ürün tipi başarıyla oluşturuldu/güncellendi</response>
    /// <response code="400">Geçersiz istek verisi veya özellik bulunamadı</response>
    /// <response code="409">Ürün tipi kodu zaten kullanımda</response>
    [HttpPost]
    [Authorize(Policy = "product-types:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpsertProductType([FromBody] ProductTypeSyncRequest request)
    {
        // Validate: Id (ExternalId) is required
        if (string.IsNullOrEmpty(request.Id))
        {
            return BadRequestResponse("Id is required", "ID_REQUIRED");
        }

        var command = new UpsertProductTypeCommand
        {
            ExternalId = request.Id,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            Attributes = request.Attributes?.Select(a => new UpsertProductTypeAttributeDto
            {
                AttributeExternalId = a.AttributeId,
                AttributeCode = a.AttributeCode,
                IsRequired = a.IsRequired,
                DisplayOrder = a.DisplayOrder
            }).ToList()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CODE_EXISTS")
            {
                return ConflictResponse(result.ErrorMessage ?? "Product type code already exists", result.ErrorCode);
            }
            if (result.ErrorCode == "ATTRIBUTE_NOT_FOUND")
            {
                return BadRequestResponse(result.ErrorMessage ?? "Attribute not found", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to upsert product type", result.ErrorCode);
        }

        _logger.LogInformation(
            "Product type {Id} synced by API client {ClientName}",
            request.Id,
            GetClientName());

        return OkResponse(MapToProductTypeDto(result.Data!));
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Ürün tipini siler (soft delete)
    /// </summary>
    /// <param name="id">Ürün tipinin harici ID'si</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <remarks>
    /// Ürün tipini ve ilişkili tüm özellik atamalarını siler.
    /// Soft delete uygulanır - kayıt veritabanından fiziksel olarak silinmez.
    ///
    /// **Uyarı:** Bu ürün tipini kullanan ürünler varsa, önce bu ürünleri başka bir tipe taşımanız gerekebilir.
    /// </remarks>
    /// <response code="200">Ürün tipi başarıyla silindi</response>
    /// <response code="404">Ürün tipi bulunamadı</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "product-types:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductType(string id)
    {
        var productType = await _unitOfWork.ProductTypes.GetByExternalIdAsync(id);

        if (productType == null)
        {
            return NotFoundResponse($"Product type with ID '{id}' not found");
        }

        var command = new DeleteProductTypeCommand(productType.Id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete product type", result.ErrorCode);
        }

        _logger.LogInformation(
            "Product type {Id} deleted by API client {ClientName}",
            id, GetClientName());

        return OkResponse<object?>(null, "Product type deleted successfully");
    }

    #endregion

    #region Private Mapping Methods

    private static ProductTypeListDto MapToProductTypeListDto(Application.DTOs.ProductTypes.ProductTypeListDto source)
    {
        return new ProductTypeListDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.Code,
            Name = source.Name,
            IsActive = source.IsActive,
            AttributeCount = source.AttributeCount,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static ProductTypeDto MapToProductTypeDto(Application.DTOs.ProductTypes.ProductTypeDto source)
    {
        return new ProductTypeDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.Code,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            AttributeCount = source.Attributes.Count,
            Attributes = source.Attributes.Select(MapToProductTypeAttributeDto).ToList(),
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static ProductTypeDto MapEntityToDto(Domain.Entities.ProductType entity)
    {
        return new ProductTypeDto
        {
            Id = entity.ExternalId ?? string.Empty,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            AttributeCount = entity.Attributes.Count,
            Attributes = entity.Attributes.Select(a => new ProductTypeAttributeDto
            {
                AttributeId = a.AttributeDefinition?.ExternalId ?? string.Empty,
                Code = a.AttributeDefinition?.Code ?? string.Empty,
                Name = a.AttributeDefinition?.Name ?? string.Empty,
                Type = a.AttributeDefinition?.Type.ToString() ?? string.Empty,
                Unit = a.AttributeDefinition?.Unit,
                IsRequired = a.IsRequired,
                DisplayOrder = a.DisplayOrder,
                PredefinedValues = a.AttributeDefinition?.PredefinedValues.Select(v => new AttributeValueOptionDto
                {
                    Value = v.Value,
                    DisplayText = v.DisplayText,
                    DisplayOrder = v.DisplayOrder
                }).ToList() ?? new()
            }).ToList(),
            LastSyncedAt = entity.LastSyncedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static ProductTypeAttributeDto MapToProductTypeAttributeDto(
        Application.DTOs.ProductTypes.ProductTypeAttributeDto source)
    {
        return new ProductTypeAttributeDto
        {
            AttributeId = source.AttributeExternalId ?? string.Empty,
            Code = source.AttributeCode,
            Name = source.AttributeName,
            Type = source.AttributeType.ToString(),
            Unit = source.Unit,
            IsRequired = source.IsRequired,
            DisplayOrder = source.DisplayOrder,
            PredefinedValues = source.PredefinedValues?.Select(MapToAttributeValueOptionDto).ToList() ?? new()
        };
    }

    private static AttributeValueOptionDto MapToAttributeValueOptionDto(
        Application.DTOs.Attributes.AttributeValueDto source)
    {
        return new AttributeValueOptionDto
        {
            Value = source.Value,
            DisplayText = source.DisplayText,
            DisplayOrder = source.DisplayOrder
        };
    }

    #endregion
}
