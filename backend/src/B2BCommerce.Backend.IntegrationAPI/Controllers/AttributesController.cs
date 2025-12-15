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
/// Integration API controller for attribute definition management.
/// All operations use ExternalId as the primary identifier.
/// </summary>
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
    /// Get all attribute definitions with filtering and pagination
    /// </summary>
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
    /// Get attribute definition by ID (ExternalId)
    /// </summary>
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
    /// Get specific predefined value by attribute ID (ExternalId) and value string
    /// </summary>
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
    /// Upserts attribute definition. If attribute with given ID (ExternalId) exists, it is updated; otherwise, a new attribute is created.
    /// Id is required for creating new attributes.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
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
            }).ToList(),
            ModifiedBy = GetClientName()
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
    /// Upserts a predefined value for an attribute (by attribute ID - ExternalId)
    /// </summary>
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
            DisplayOrder = request.DisplayOrder,
            ModifiedBy = GetClientName()
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
    /// Delete an attribute definition by ID (ExternalId) - soft delete
    /// </summary>
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
    /// Delete a predefined value from an attribute (by attribute ID - ExternalId and value string)
    /// </summary>
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
