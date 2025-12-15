using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeByCode;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeById;
using B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;
using B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Product Types API endpoints for external integrations.
/// Provides read access to product types and their attribute definitions.
/// </summary>
public class ProductTypesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductTypesController> _logger;

    public ProductTypesController(
        IMediator mediator,
        ILogger<ProductTypesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all product types with optional filtering.
    /// Used by ERP systems to synchronize product type definitions.
    /// </summary>
    /// <param name="filter">Filter parameters</param>
    /// <returns>List of product types</returns>
    [HttpGet]
    [Authorize(Policy = "products:read")]
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
    [Authorize(Policy = "products:read")]
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
    /// Get a product type by code with full attribute details.
    /// Returns the complete attribute schema for a product type.
    /// </summary>
    /// <param name="code">Product type code (e.g., "memory_card")</param>
    /// <returns>Product type with attributes</returns>
    [HttpGet("code/{code}")]
    [Authorize(Policy = "products:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<ProductTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductTypeByCode(string code)
    {
        var query = new GetProductTypeByCodeQuery(code);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PRODUCT_TYPE_NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Product type with code '{code}' not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get product type", result.ErrorCode);
        }

        return OkResponse(MapToProductTypeDto(result.Data!));
    }

    #region Private Mapping Methods

    private static ProductTypeListDto MapToProductTypeListDto(Application.DTOs.ProductTypes.ProductTypeListDto source)
    {
        return new ProductTypeListDto
        {
            Id = source.Id,
            Code = source.Code,
            Name = source.Name,
            IsActive = source.IsActive,
            AttributeCount = source.AttributeCount
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
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
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
            NameEn = null, // Not exposed in Application DTO
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
