using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.DeleteAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.RemoveAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeDefinition;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeValue;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionById;
using B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Integration API controller for attribute definition management
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
                a.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                (a.NameEn?.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ?? false));
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
    /// Get attribute definition by internal ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttribute(Guid id)
    {
        var query = new GetAttributeDefinitionByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Attribute with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get attribute", result.ErrorCode);
        }

        return OkResponse(MapToAttributeDefinitionDto(result.Data!));
    }

    /// <summary>
    /// Get attribute definition by external ID (primary lookup)
    /// </summary>
    [HttpGet("ext/{extId}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeByExtId(string extId)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(extId);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with external ID '{extId}' not found");
        }

        return OkResponse(MapEntityToDto(attribute));
    }

    /// <summary>
    /// Get specific predefined value by attribute ID and value ID
    /// </summary>
    [HttpGet("{id:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeValue(Guid id, Guid valueId)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(id);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with ID {id} not found");
        }

        var value = attribute.PredefinedValues.FirstOrDefault(v => v.Id == valueId);
        if (value == null)
        {
            return NotFoundResponse($"Value with ID {valueId} not found");
        }

        return OkResponse(MapToAttributeValueDto(value));
    }

    /// <summary>
    /// Get specific predefined value by attribute external ID and value ID
    /// </summary>
    [HttpGet("ext/{extId}/values/{valueId:guid}")]
    [Authorize(Policy = "attributes:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeValueByExtId(string extId, Guid valueId)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(extId);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with external ID '{extId}' not found");
        }

        var value = attribute.PredefinedValues.FirstOrDefault(v => v.Id == valueId);
        if (value == null)
        {
            return NotFoundResponse($"Value with ID {valueId} not found");
        }

        return OkResponse(MapToAttributeValueDto(value));
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Upserts attribute definition. If attribute with given external ID or internal ID exists, it is updated; otherwise, a new attribute is created.
    /// One of ExtId or Id is required. If only Id is provided, ExtId will be set to Id.ToString().
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertAttribute([FromBody] AttributeSyncRequest request)
    {
        // Validate: One of ExtId or Id is required
        if (string.IsNullOrEmpty(request.ExtId) && !request.Id.HasValue)
        {
            return BadRequestResponse("One of ExtId or Id is required", "ID_REQUIRED");
        }

        // Parse AttributeType
        if (!Enum.TryParse<AttributeType>(request.Type, true, out var attributeType))
        {
            return BadRequestResponse(
                $"Invalid attribute type '{request.Type}'. Valid values: Text, Number, Select, MultiSelect, Boolean, Date",
                "INVALID_TYPE");
        }

        // If only Id is provided, set ExtId to Id.ToString()
        var externalId = request.ExtId;
        if (string.IsNullOrEmpty(externalId) && request.Id.HasValue)
        {
            externalId = request.Id.Value.ToString();
        }

        var command = new UpsertAttributeDefinitionCommand
        {
            Id = request.Id,
            ExternalId = externalId,
            ExternalCode = request.ExtCode,
            Code = request.Code,
            Name = request.Name,
            NameEn = request.NameEn,
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
            "Attribute {ExternalId} synced by API client {ClientName}",
            externalId,
            GetClientName());

        return OkResponse(MapToAttributeDefinitionDto(result.Data!));
    }

    /// <summary>
    /// Upserts a predefined value for an attribute (by attribute ID)
    /// </summary>
    [HttpPost("{id:guid}/values")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertAttributeValue(Guid id, [FromBody] AttributeValueSyncRequest request)
    {
        var command = new UpsertAttributeValueCommand
        {
            AttributeDefinitionId = id,
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
                return NotFoundResponse(result.ErrorMessage ?? $"Attribute with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to upsert value", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute value '{Value}' synced for attribute {AttributeId} by API client {ClientName}",
            request.Value, id, GetClientName());

        return OkResponse(MapToAttributeValueDto(result.Data!));
    }

    /// <summary>
    /// Upserts a predefined value for an attribute (by attribute external ID)
    /// </summary>
    [HttpPost("ext/{extId}/values")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<AttributeValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertAttributeValueByExtId(string extId, [FromBody] AttributeValueSyncRequest request)
    {
        var command = new UpsertAttributeValueCommand
        {
            AttributeExternalId = extId,
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
                return NotFoundResponse(result.ErrorMessage ?? $"Attribute with external ID '{extId}' not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to upsert value", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute value '{Value}' synced for attribute {ExternalId} by API client {ClientName}",
            request.Value, extId, GetClientName());

        return OkResponse(MapToAttributeValueDto(result.Data!));
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Delete an attribute definition (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttribute(Guid id)
    {
        var command = new DeleteAttributeDefinitionCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Attribute with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete attribute", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute {AttributeId} deleted by API client {ClientName}",
            id, GetClientName());

        return OkResponse<object?>(null, "Attribute deleted successfully");
    }

    /// <summary>
    /// Delete an attribute definition by external ID (soft delete)
    /// </summary>
    [HttpDelete("ext/{extId}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeByExtId(string extId)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetByExternalIdAsync(extId);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with external ID '{extId}' not found");
        }

        var command = new DeleteAttributeDefinitionCommand(attribute.Id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete attribute", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute with external ID {ExternalId} deleted by API client {ClientName}",
            extId, GetClientName());

        return OkResponse<object?>(null, "Attribute deleted successfully");
    }

    /// <summary>
    /// Delete a predefined value from an attribute (by attribute ID)
    /// </summary>
    [HttpDelete("{id:guid}/values/{valueId:guid}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeValue(Guid id, Guid valueId)
    {
        var command = new RemoveAttributeValueCommand(id, valueId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? "Attribute or value not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete value", result.ErrorCode);
        }

        _logger.LogInformation(
            "Attribute value {ValueId} deleted from attribute {AttributeId} by API client {ClientName}",
            valueId, id, GetClientName());

        return OkResponse<object?>(null, "Attribute value deleted successfully");
    }

    /// <summary>
    /// Delete a predefined value from an attribute (by attribute external ID)
    /// </summary>
    [HttpDelete("ext/{extId}/values/{valueId:guid}")]
    [Authorize(Policy = "attributes:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeValueByExtId(string extId, Guid valueId)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetByExternalIdAsync(extId);

        if (attribute == null)
        {
            return NotFoundResponse($"Attribute with external ID '{extId}' not found");
        }

        var command = new RemoveAttributeValueCommand(attribute.Id, valueId);
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
            "Attribute value {ValueId} deleted from attribute {ExternalId} by API client {ClientName}",
            valueId, extId, GetClientName());

        return OkResponse<object?>(null, "Attribute value deleted successfully");
    }

    #endregion

    #region Private Mapping Methods

    private static AttributeDefinitionDto MapToAttributeDefinitionDto(Application.DTOs.Attributes.AttributeDefinitionDto source)
    {
        return new AttributeDefinitionDto
        {
            Id = source.Id,
            Code = source.Code,
            Name = source.Name,
            NameEn = source.NameEn,
            Type = source.Type.ToString(),
            Unit = source.Unit,
            IsFilterable = source.IsFilterable,
            IsRequired = source.IsRequired,
            IsVisibleOnProductPage = source.IsVisibleOnProductPage,
            DisplayOrder = source.DisplayOrder,
            ExternalId = source.ExternalId,
            ExternalCode = source.ExternalCode,
            LastSyncedAt = source.LastSyncedAt,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            PredefinedValues = source.PredefinedValues?.Select(v => new AttributeValueDto
            {
                Id = v.Id,
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
            Id = source.Id,
            Code = source.Code,
            Name = source.Name,
            NameEn = source.NameEn,
            Type = source.Type.ToString(),
            Unit = source.Unit,
            IsFilterable = source.IsFilterable,
            IsRequired = source.IsRequired,
            IsVisibleOnProductPage = source.IsVisibleOnProductPage,
            DisplayOrder = source.DisplayOrder,
            ExternalId = source.ExternalId,
            ExternalCode = source.ExternalCode,
            LastSyncedAt = source.LastSyncedAt,
            ValueCount = source.PredefinedValues?.Count ?? 0
        };
    }

    private static AttributeDefinitionDto MapEntityToDto(Domain.Entities.AttributeDefinition entity)
    {
        return new AttributeDefinitionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            NameEn = entity.NameEn,
            Type = entity.Type.ToString(),
            Unit = entity.Unit,
            IsFilterable = entity.IsFilterable,
            IsRequired = entity.IsRequired,
            IsVisibleOnProductPage = entity.IsVisibleOnProductPage,
            DisplayOrder = entity.DisplayOrder,
            ExternalId = entity.ExternalId,
            ExternalCode = entity.ExternalCode,
            LastSyncedAt = entity.LastSyncedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            PredefinedValues = entity.PredefinedValues.Select(v => new AttributeValueDto
            {
                Id = v.Id,
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
            Id = source.Id,
            Value = source.Value,
            DisplayText = source.DisplayText,
            DisplayOrder = source.DisplayOrder
        };
    }

    private static AttributeValueDto MapToAttributeValueDto(Domain.Entities.AttributeValue entity)
    {
        return new AttributeValueDto
        {
            Id = entity.Id,
            Value = entity.Value,
            DisplayText = entity.DisplayText,
            DisplayOrder = entity.DisplayOrder
        };
    }

    #endregion
}
