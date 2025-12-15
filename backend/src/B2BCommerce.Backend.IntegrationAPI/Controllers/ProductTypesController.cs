using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.DeleteProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpsertProductType;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeByCode;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeById;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Product Types API endpoints for external integrations.
/// Provides CRUD operations for product types and their attribute definitions.
/// </summary>
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
    /// Get all product types with optional filtering.
    /// Used by ERP systems to synchronize product type definitions.
    /// </summary>
    /// <param name="filter">Filter parameters</param>
    /// <returns>List of product types</returns>
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
    /// Get a product type by ID with full attribute details.
    /// Returns the complete attribute schema for a product type.
    /// </summary>
    /// <param name="id">Product type ID</param>
    /// <returns>Product type with attributes</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "product-types:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductType(Guid id)
    {
        var query = new GetProductTypeByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PRODUCT_TYPE_NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Product type with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get product type", result.ErrorCode);
        }

        return OkResponse(MapToProductTypeDto(result.Data!));
    }

    /// <summary>
    /// Get a product type by external ID with full attribute details.
    /// Returns the complete attribute schema for a product type.
    /// </summary>
    /// <param name="extId">Product type external ID</param>
    /// <returns>Product type with attributes</returns>
    [HttpGet("ext/{extId}")]
    [Authorize(Policy = "product-types:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductTypeByExtId(string extId)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesByExternalIdAsync(extId);

        if (productType == null)
        {
            return NotFoundResponse($"Product type with external ID '{extId}' not found");
        }

        return OkResponse(MapEntityToDto(productType));
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Upserts a product type. If product type with given external ID or internal ID exists, it is updated; otherwise, a new product type is created.
    /// One of ExtId or Id is required. If only Id is provided, ExtId will be set to Id.ToString().
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "product-types:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertProductType([FromBody] ProductTypeSyncRequest request)
    {
        // Validate: One of ExtId or Id is required
        if (string.IsNullOrEmpty(request.ExtId) && !request.Id.HasValue)
        {
            return BadRequestResponse("One of ExtId or Id is required", "ID_REQUIRED");
        }

        // If only Id is provided, set ExtId to Id.ToString()
        var externalId = request.ExtId;
        if (string.IsNullOrEmpty(externalId) && request.Id.HasValue)
        {
            externalId = request.Id.Value.ToString();
        }

        var command = new UpsertProductTypeCommand
        {
            Id = request.Id,
            ExternalId = externalId,
            ExternalCode = request.ExtCode,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            Attributes = request.Attributes?.Select(a => new UpsertProductTypeAttributeDto
            {
                AttributeDefinitionId = a.AttributeId,
                AttributeExternalId = a.AttributeExtId,
                AttributeCode = a.AttributeCode,
                IsRequired = a.IsRequired,
                DisplayOrder = a.DisplayOrder
            }).ToList(),
            ModifiedBy = GetClientName()
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
            "Product type {ExternalId} synced by API client {ClientName}",
            externalId,
            GetClientName());

        return OkResponse(MapToProductTypeDto(result.Data!));
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Delete a product type (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "product-types:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductType(Guid id)
    {
        var command = new DeleteProductTypeCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Product type with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete product type", result.ErrorCode);
        }

        _logger.LogInformation(
            "Product type {ProductTypeId} deleted by API client {ClientName}",
            id, GetClientName());

        return OkResponse<object?>(null, "Product type deleted successfully");
    }

    /// <summary>
    /// Delete a product type by external ID (soft delete)
    /// </summary>
    [HttpDelete("ext/{extId}")]
    [Authorize(Policy = "product-types:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductTypeByExtId(string extId)
    {
        var productType = await _unitOfWork.ProductTypes.GetByExternalIdAsync(extId);

        if (productType == null)
        {
            return NotFoundResponse($"Product type with external ID '{extId}' not found");
        }

        var command = new DeleteProductTypeCommand(productType.Id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete product type", result.ErrorCode);
        }

        _logger.LogInformation(
            "Product type with external ID {ExternalId} deleted by API client {ClientName}",
            extId, GetClientName());

        return OkResponse<object?>(null, "Product type deleted successfully");
    }

    #endregion

    #region Private Mapping Methods

    private static ProductTypeListDto MapToProductTypeListDto(Application.DTOs.ProductTypes.ProductTypeListDto source)
    {
        return new ProductTypeListDto
        {
            Id = source.Id,
            Code = source.Code,
            Name = source.Name,
            IsActive = source.IsActive,
            AttributeCount = source.AttributeCount,
            ExtId = source.ExternalId,
            ExtCode = source.ExternalCode,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static ProductTypeDto MapToProductTypeDto(Application.DTOs.ProductTypes.ProductTypeDto source)
    {
        return new ProductTypeDto
        {
            Id = source.Id,
            Code = source.Code,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            AttributeCount = source.Attributes.Count,
            Attributes = source.Attributes.Select(MapToProductTypeAttributeDto).ToList(),
            ExtId = source.ExternalId,
            ExtCode = source.ExternalCode,
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static ProductTypeDto MapEntityToDto(Domain.Entities.ProductType entity)
    {
        return new ProductTypeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            AttributeCount = entity.Attributes.Count,
            Attributes = entity.Attributes.Select(a => new ProductTypeAttributeDto
            {
                AttributeDefinitionId = a.AttributeDefinitionId,
                Code = a.AttributeDefinition?.Code ?? string.Empty,
                Name = a.AttributeDefinition?.Name ?? string.Empty,
                NameEn = null,
                Type = a.AttributeDefinition?.Type.ToString() ?? string.Empty,
                Unit = a.AttributeDefinition?.Unit,
                IsRequired = a.IsRequired,
                DisplayOrder = a.DisplayOrder,
                PredefinedValues = a.AttributeDefinition?.PredefinedValues.Select(v => new AttributeValueOptionDto
                {
                    Id = v.Id,
                    Value = v.Value,
                    DisplayText = v.DisplayText,
                    DisplayOrder = v.DisplayOrder
                }).ToList() ?? new()
            }).ToList(),
            ExtId = entity.ExternalId,
            ExtCode = entity.ExternalCode,
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
            AttributeDefinitionId = source.AttributeDefinitionId,
            Code = source.AttributeCode,
            Name = source.AttributeName,
            NameEn = null,
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
            Id = source.Id,
            Value = source.Value,
            DisplayText = source.DisplayText,
            DisplayOrder = source.DisplayOrder
        };
    }

    #endregion
}
