using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeDefinition;

/// <summary>
/// Handler for UpsertAttributeDefinitionCommand.
/// Creates or updates an attribute definition based on Id, ExternalId, or Code.
/// </summary>
public class UpsertAttributeDefinitionCommandHandler : ICommandHandler<UpsertAttributeDefinitionCommand, Result<AttributeDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpsertAttributeDefinitionCommandHandler> _logger;

    public UpsertAttributeDefinitionCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpsertAttributeDefinitionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AttributeDefinitionDto>> Handle(UpsertAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        // 1. Find existing attribute (by Id → ExternalId → Code)
        AttributeDefinition? attribute = null;
        bool createWithSpecificId = false;

        if (request.Id.HasValue)
        {
            attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(request.Id.Value, cancellationToken);

            // If Id is provided but not found, we'll create with that specific Id
            if (attribute == null)
            {
                createWithSpecificId = true;
            }
        }

        // If not found by Id, try ExternalId
        if (attribute == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalId))
        {
            attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(request.ExternalId, cancellationToken);
        }

        // If still not found, try Code as fallback
        if (attribute == null && !createWithSpecificId)
        {
            attribute = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(request.Code, cancellationToken);
        }

        // 2. Create or Update
        if (attribute == null)
        {
            // Validate ExternalId is provided for new creation (unless Id-based creation)
            var externalId = request.ExternalId;
            if (string.IsNullOrEmpty(externalId) && request.Id.HasValue)
            {
                externalId = request.Id.Value.ToString();
            }

            if (string.IsNullOrEmpty(externalId))
            {
                return Result<AttributeDefinitionDto>.Failure(
                    "ExternalId or Id is required for creating new attribute definition",
                    "EXTERNAL_ID_REQUIRED");
            }

            // Check if code already exists (case-insensitive)
            var existingByCode = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(request.Code, cancellationToken);
            if (existingByCode != null)
            {
                return Result<AttributeDefinitionDto>.Failure(
                    $"An attribute definition with code '{request.Code}' already exists",
                    "CODE_EXISTS");
            }

            attribute = AttributeDefinition.CreateFromExternal(
                externalId: externalId,
                code: request.Code,
                name: request.Name,
                type: request.Type,
                nameEn: request.NameEn,
                unit: request.Unit,
                isFilterable: request.IsFilterable,
                isRequired: request.IsRequired,
                isVisibleOnProductPage: request.IsVisibleOnProductPage,
                displayOrder: request.DisplayOrder,
                externalCode: request.ExternalCode,
                specificId: createWithSpecificId ? request.Id : null);

            attribute.CreatedBy = request.ModifiedBy;

            // Add predefined values for Select/MultiSelect types
            if (request.PredefinedValues != null && attribute.RequiresPredefinedValues())
            {
                foreach (var valueDto in request.PredefinedValues)
                {
                    attribute.AddPredefinedValue(valueDto.Value, valueDto.DisplayText, valueDto.DisplayOrder);
                }
            }

            await _unitOfWork.AttributeDefinitions.AddAsync(attribute, cancellationToken);

            _logger.LogInformation(
                "Creating attribute definition from external sync: {ExternalId} - {Code} (Id: {Id})",
                externalId, request.Code, attribute.Id);
        }
        else
        {
            // Update existing attribute
            attribute.UpdateFromExternal(
                name: request.Name,
                nameEn: request.NameEn,
                unit: request.Unit,
                isFilterable: request.IsFilterable,
                isRequired: request.IsRequired,
                isVisibleOnProductPage: request.IsVisibleOnProductPage,
                displayOrder: request.DisplayOrder,
                externalCode: request.ExternalCode);

            attribute.UpdatedBy = request.ModifiedBy;

            // Sync predefined values for Select/MultiSelect types (full replacement)
            if (attribute.RequiresPredefinedValues())
            {
                SyncPredefinedValues(attribute, request.PredefinedValues);
            }

            _logger.LogInformation(
                "Updating attribute definition from external sync: {ExternalId} - {Code} (Id: {Id})",
                attribute.ExternalId, request.Code, attribute.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with predefined values
        var result = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(attribute.Id, cancellationToken);
        return Result<AttributeDefinitionDto>.Success(_mapper.Map<AttributeDefinitionDto>(result));
    }

    /// <summary>
    /// Syncs predefined values using full replacement semantics.
    /// Values are matched by their Value property (case-insensitive).
    /// </summary>
    private void SyncPredefinedValues(AttributeDefinition attribute, List<UpsertAttributeValueDto>? newValues)
    {
        if (newValues == null || newValues.Count == 0)
        {
            // Clear all values if empty list provided
            attribute.ClearPredefinedValues();
            return;
        }

        var existingValues = attribute.PredefinedValues.ToList();
        var newValueKeys = newValues.Select(v => v.Value.ToLowerInvariant()).ToHashSet();

        // Remove values that are not in the new list
        foreach (var existingValue in existingValues)
        {
            if (!newValueKeys.Contains(existingValue.Value.ToLowerInvariant()))
            {
                attribute.RemovePredefinedValue(existingValue.Id);
            }
        }

        // Add or update values
        foreach (var newValue in newValues)
        {
            var existing = attribute.FindPredefinedValueByValue(newValue.Value);
            if (existing != null)
            {
                // Update existing
                attribute.UpdatePredefinedValue(existing.Id, newValue.Value, newValue.DisplayText, newValue.DisplayOrder);
            }
            else
            {
                // Add new
                attribute.AddPredefinedValue(newValue.Value, newValue.DisplayText, newValue.DisplayOrder);
            }
        }
    }
}
