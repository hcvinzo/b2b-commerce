using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.DeleteAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.RemoveAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Özellik (Attribute) tanımları API uç noktaları - harici entegrasyonlar için.
/// Tüm işlemler birincil tanımlayıcı olarak ExternalId kullanır.
/// </summary>
/// <remarks>
/// Bu API, ürün özelliklerinin (RAM, ekran boyutu, renk vb.) yönetimi için kullanılır.
/// Özellikler ürün tiplerine atanır ve ürünlerde değerler girilir.
/// Select/MultiSelect tipleri için önceden tanımlı değerler desteklenir.
/// </remarks>
[Route("api/v1/attributes")]
public class AttributesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AttributesController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AttributesController(
        IMediator mediator,
        ILogger<AttributesController> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Read Operations

    /// <summary>
    /// Tüm özellik tanımlarını filtreleme ve sayfalama ile getirir
    /// </summary>
    /// <param name="filter">Filtreleme parametreleri (arama, tip, filtrelenebilirlik, sayfalama)</param>
    /// <returns>Sayfalanmış özellik tanımları listesi</returns>
    /// <response code="200">Özellik tanımları başarıyla getirildi</response>
    [HttpGet]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.PagedApiResponse<AttributeDefinitionListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttributes([FromQuery] AttributeFilterDto filter)
    {
        var query = new GetAttributeDefinitionsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get attributes", result.ErrorCode);
        }

        var attributes = result.Data!.AsEnumerable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLowerInvariant();
            attributes = attributes.Where(a =>
                a.Code.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                a.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.IsFilterable.HasValue)
        {
            attributes = attributes.Where(a => a.IsFilterable == filter.IsFilterable.Value);
        }

        if (!string.IsNullOrEmpty(filter.Type) && Enum.TryParse<AttributeType>(filter.Type, true, out var typeFilter))
        {
            attributes = attributes.Where(a => a.Type == typeFilter);
        }

        // Apply sorting
        attributes = filter.SortBy?.ToLowerInvariant() switch
        {
            "code" => filter.SortDirection?.ToLowerInvariant() == "desc"
                ? attributes.OrderByDescending(a => a.Code)
                : attributes.OrderBy(a => a.Code),
            "name" => filter.SortDirection?.ToLowerInvariant() == "desc"
                ? attributes.OrderByDescending(a => a.Name)
                : attributes.OrderBy(a => a.Name),
            "createdat" => filter.SortDirection?.ToLowerInvariant() == "desc"
                ? attributes.OrderByDescending(a => a.CreatedAt)
                : attributes.OrderBy(a => a.CreatedAt),
            _ => filter.SortDirection?.ToLowerInvariant() == "desc"
                ? attributes.OrderByDescending(a => a.DisplayOrder)
                : attributes.OrderBy(a => a.DisplayOrder)
        };

        var totalCount = attributes.Count();
        var pagedData = attributes
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        if (filter.IncludeValues)
        {
            var dtos = pagedData.Select(MapToAttributeDefinitionDto).ToList();
            return PagedResponse(dtos, filter.PageNumber, filter.PageSize, totalCount);
        }
        else
        {
            var dtos = pagedData.Select(MapToAttributeDefinitionListDto).ToList();
            return PagedResponse(dtos, filter.PageNumber, filter.PageSize, totalCount);
        }
    }

    /// <summary>
    /// Belirtilen ID'ye (ExternalId) sahip özellik tanımını getirir
    /// </summary>
    /// <param name="id">Özellik tanımının harici ID'si</param>
    /// <returns>Özellik tanımı detayları (önceden tanımlı değerler dahil)</returns>
    /// <response code="200">Özellik tanımı başarıyla getirildi</response>
    /// <response code="404">Özellik tanımı bulunamadı</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttribute(string id)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(id);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with ID '{id}' not found");
        }

        return OkResponse(MapEntityToDto(attribute));
    }

    /// <summary>
    /// Belirtilen özelliğin belirli bir önceden tanımlı değerini getirir
    /// </summary>
    /// <param name="id">Özellik tanımının harici ID'si</param>
    /// <param name="value">Değer anahtarı</param>
    /// <returns>Önceden tanımlı değer detayları</returns>
    /// <response code="200">Değer başarıyla getirildi</response>
    /// <response code="404">Özellik veya değer bulunamadı</response>
    [HttpGet("{id}/values/{value}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeValue(string id, string value)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(id);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with ID '{id}' not found");
        }

        var attributeValue = attribute.PredefinedValues.FirstOrDefault(v =>
            v.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (attributeValue == null)
        {
            return NotFoundResponse($"Value '{value}' not found");
        }

        return OkResponse(MapToAttributeValueDto(attributeValue));
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Özellik tanımını oluşturur veya günceller (Upsert)
    /// </summary>
    /// <param name="request">Özellik tanımı verileri</param>
    /// <returns>Oluşturulan veya güncellenen özellik tanımı</returns>
    /// <remarks>
    /// Belirtilen ID (ExternalId) ile özellik varsa güncellenir, yoksa yeni özellik oluşturulur.
    ///
    /// **Önemli Notlar:**
    /// - Id (ExternalId) yeni özellik oluşturmak için zorunludur
    /// - Code alanı sistem genelinde benzersiz olmalıdır
    /// - Type alanı: Text, Number, Select, MultiSelect, Boolean, Date değerlerinden biri olmalıdır
    /// - Select/MultiSelect tipleri için Values dizisi ile önceden tanımlı değerler eklenebilir
    /// - Values gönderildiğinde, mevcut değerler tamamen değiştirilir (full replacement)
    ///
    /// **Örnek İstek:**
    /// ```json
    /// {
    ///   "id": "ATTR-RAM",
    ///   "code": "ram",
    ///   "name": "RAM Kapasitesi",
    ///   "type": "Select",
    ///   "unit": "GB",
    ///   "isFilterable": true,
    ///   "values": [
    ///     { "value": "8", "displayText": "8 GB", "displayOrder": 1 },
    ///     { "value": "16", "displayText": "16 GB", "displayOrder": 2 }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Özellik başarıyla oluşturuldu/güncellendi</response>
    /// <response code="400">Geçersiz istek verisi veya tip değeri</response>
    /// <response code="409">Özellik kodu zaten kullanımda</response>
    [HttpPost]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpsertAttribute([FromBody] AttributeSyncRequest request)
    {
        // Validate: Id (ExternalId) is required
        if (string.IsNullOrEmpty(request.Id))
        {
            return BadRequestResponse("Id is required", "ID_REQUIRED");
        }

        // Parse AttributeType
        if (!Enum.TryParse<AttributeType>(request.Type, true, out var attributeType))
        {
            return BadRequestResponse(
                $"Invalid attribute type '{request.Type}'. Valid values: Text, Number, Select, MultiSelect, Boolean, Date",
                "INVALID_TYPE");
        }

        var command = new UpsertAttributeDefinitionCommand
        {
            ExternalId = request.Id,
            ExternalCode = null, // Not exposed in Integration API
            Code = request.Code,
            Name = request.Name,
            Type = attributeType,
            Unit = request.Unit,
            IsFilterable = request.IsFilterable,
            IsRequired = request.IsRequired,
            IsVisibleOnProductPage = request.IsVisibleOnProductPage,
            DisplayOrder = request.DisplayOrder,
            PredefinedValues = request.Values?.Select(v => new UpsertAttributeValueDto
            {
                Value = v.Value,
                DisplayText = v.DisplayText,
                DisplayOrder = v.DisplayOrder
            }).ToList()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CODE_EXISTS")
            {
                return ConflictResponse(result.ErrorMessage ?? "Attribute code already exists", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to upsert attribute", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute {Id} synced by API client {ClientName}",
            request.Id,
            GetClientName());

        return OkResponse(MapToAttributeDefinitionDto(result.Data!));
    }

    /// <summary>
    /// Özelliğe önceden tanımlı değer ekler veya günceller (Upsert)
    /// </summary>
    /// <param name="id">Özellik tanımının harici ID'si</param>
    /// <param name="request">Değer verileri</param>
    /// <returns>Oluşturulan veya güncellenen değer</returns>
    /// <remarks>
    /// Select ve MultiSelect tipindeki özellikler için önceden tanımlı değerler ekler.
    /// Aynı value değeri varsa güncellenir, yoksa yeni değer oluşturulur.
    ///
    /// **Örnek İstek:**
    /// ```json
    /// {
    ///   "value": "32",
    ///   "displayText": "32 GB",
    ///   "displayOrder": 3
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Değer başarıyla oluşturuldu/güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Özellik bulunamadı</response>
    [HttpPost("{id}/values")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertAttributeValue(string id, [FromBody] AttributeValueSyncRequest request)
    {
        var command = new UpsertAttributeValueCommand
        {
            AttributeExternalId = id,
            Value = request.Value,
            DisplayText = request.DisplayText,
            DisplayOrder = request.DisplayOrder
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Attribute with ID '{id}' not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to upsert value", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute value '{Value}' synced for attribute {Id} by API client {ClientName}",
            request.Value, id, GetClientName());

        return OkResponse(MapToAttributeValueDto(result.Data!));
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Özellik tanımını siler (soft delete)
    /// </summary>
    /// <param name="id">Özellik tanımının harici ID'si</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <remarks>
    /// Özellik tanımını ve ilişkili tüm önceden tanımlı değerlerini siler.
    /// Soft delete uygulanır - kayıt veritabanından fiziksel olarak silinmez.
    ///
    /// **Uyarı:** Bu özelliği kullanan ürünler varsa, özellik değerleri de etkilenebilir.
    /// </remarks>
    /// <response code="200">Özellik başarıyla silindi</response>
    /// <response code="404">Özellik bulunamadı</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttribute(string id)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetByExternalIdAsync(id);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with ID '{id}' not found");
        }

        var command = new DeleteAttributeDefinitionCommand(attribute.Id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete attribute", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute {Id} deleted by API client {ClientName}",
            id, GetClientName());

        return OkResponse<object?>(null, "Attribute deleted successfully");
    }

    /// <summary>
    /// Özellikten önceden tanımlı değer siler
    /// </summary>
    /// <param name="id">Özellik tanımının harici ID'si</param>
    /// <param name="value">Silinecek değer anahtarı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <remarks>
    /// Belirtilen özellikten belirtilen önceden tanımlı değeri siler.
    /// Değer karşılaştırması büyük/küçük harf duyarsızdır.
    ///
    /// **Uyarı:** Bu değeri kullanan ürünler varsa, ürünlerdeki özellik değerleri de etkilenebilir.
    /// </remarks>
    /// <response code="200">Değer başarıyla silindi</response>
    /// <response code="404">Özellik veya değer bulunamadı</response>
    [HttpDelete("{id}/values/{value}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeValue(string id, string value)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(id);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with ID '{id}' not found");
        }

        var attributeValue = attribute.PredefinedValues.FirstOrDefault(v =>
            v.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (attributeValue == null)
        {
            return NotFoundResponse($"Value '{value}' not found");
        }

        var command = new RemoveAttributeValueCommand(attribute.Id, attributeValue.Id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? "Value not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete value", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute value '{Value}' deleted from attribute {Id} by API client {ClientName}",
            value, id, GetClientName());

        return OkResponse<object?>(null, "Attribute value deleted successfully");
    }

    #endregion

    #region Private Mapping Methods

    private static AttributeDefinitionDto MapToAttributeDefinitionDto(Application.DTOs.Attributes.AttributeDefinitionDto source)
    {
        return new AttributeDefinitionDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.Code,
            Name = source.Name,
            Type = source.Type.ToString(),
            Unit = source.Unit,
            IsFilterable = source.IsFilterable,
            IsRequired = source.IsRequired,
            IsVisibleOnProductPage = source.IsVisibleOnProductPage,
            DisplayOrder = source.DisplayOrder,
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            PredefinedValues = source.PredefinedValues?.Select(v => new AttributeValueDto
            {
                Value = v.Value,
                DisplayText = v.DisplayText,
                DisplayOrder = v.DisplayOrder
            }).ToList()
        };
    }

    private static AttributeDefinitionListDto MapToAttributeDefinitionListDto(Application.DTOs.Attributes.AttributeDefinitionDto source)
    {
        return new AttributeDefinitionListDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.Code,
            Name = source.Name,
            Type = source.Type.ToString(),
            Unit = source.Unit,
            IsFilterable = source.IsFilterable,
            IsRequired = source.IsRequired,
            IsVisibleOnProductPage = source.IsVisibleOnProductPage,
            DisplayOrder = source.DisplayOrder,
            LastSyncedAt = source.LastSyncedAt,
            ValueCount = source.PredefinedValues?.Count ?? 0
        };
    }

    private static AttributeDefinitionDto MapEntityToDto(Domain.Entities.AttributeDefinition entity)
    {
        return new AttributeDefinitionDto
        {
            Id = entity.ExternalId ?? string.Empty,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type.ToString(),
            Unit = entity.Unit,
            IsFilterable = entity.IsFilterable,
            IsRequired = entity.IsRequired,
            IsVisibleOnProductPage = entity.IsVisibleOnProductPage,
            DisplayOrder = entity.DisplayOrder,
            LastSyncedAt = entity.LastSyncedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            PredefinedValues = entity.PredefinedValues.Select(v => new AttributeValueDto
            {
                Value = v.Value,
                DisplayText = v.DisplayText,
                DisplayOrder = v.DisplayOrder
            }).ToList()
        };
    }

    private static AttributeValueDto MapToAttributeValueDto(Application.DTOs.Attributes.AttributeValueDto source)
    {
        return new AttributeValueDto
        {
            Value = source.Value,
            DisplayText = source.DisplayText,
            DisplayOrder = source.DisplayOrder
        };
    }

    private static AttributeValueDto MapToAttributeValueDto(Domain.Entities.AttributeValue entity)
    {
        return new AttributeValueDto
        {
            Value = entity.Value,
            DisplayText = entity.DisplayText,
            DisplayOrder = entity.DisplayOrder
        };
    }

    #endregion
}
