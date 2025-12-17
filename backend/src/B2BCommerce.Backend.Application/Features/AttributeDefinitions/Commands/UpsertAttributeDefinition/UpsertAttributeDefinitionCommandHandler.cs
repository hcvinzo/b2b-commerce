using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Common.Helpers;
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
        // 1. Use shared lookup helper: ExternalId → Code
        var lookup = await ExternalEntityLookupExtensions.LookupExternalEntityAsync(
            request.ExternalId,
            request.Code,  // fallback key
            (extId, ct) => _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(extId, ct),
            (code, ct) => _unitOfWork.AttributeDefinitions.GetByCodeAsync(code, ct),
            cancellationToken);

        AttributeDefinition attribute;

        // 2. Create or Update
        if (lookup.IsNew)
        {
            // Check if code already exists (case-insensitive)
            var existingByCode = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(request.Code, cancellationToken);
            if (existingByCode is not null)
            {
                return Result<AttributeDefinitionDto>.Failure(
                    $"An attribute definition with code '{request.Code}' already exists",
                    "CODE_EXISTS");
            }

            // If ExternalId provided, use CreateFromExternal; otherwise use Create (auto-generates ExternalId)
            if (!string.IsNullOrEmpty(lookup.EffectiveExternalId))
            {
                attribute = AttributeDefinition.CreateFromExternal(
                    externalId: lookup.EffectiveExternalId,
                    code: request.Code,
                    name: request.Name,
                    type: request.Type,
                    unit: request.Unit,
                    isFilterable: request.IsFilterable,
                    isRequired: request.IsRequired,
                    isVisibleOnProductPage: request.IsVisibleOnProductPage,
                    displayOrder: request.DisplayOrder,
                    externalCode: request.ExternalCode);
            }
            else
            {
                attribute = AttributeDefinition.Create(
                    code: request.Code,
                    name: request.Name,
                    type: request.Type,
                    unit: request.Unit,
                    isFilterable: request.IsFilterable,
                    isRequired: request.IsRequired,
                    isVisibleOnProductPage: request.IsVisibleOnProductPage,
                    displayOrder: request.DisplayOrder);
            }

            // Add predefined values for Select/MultiSelect types
            if (request.PredefinedValues is not null && attribute.RequiresPredefinedValues())
            {
                foreach (var valueDto in request.PredefinedValues)
                {
                    attribute.AddPredefinedValue(valueDto.Value, valueDto.DisplayText, valueDto.DisplayOrder);
                }
            }

            await _unitOfWork.AttributeDefinitions.AddAsync(attribute, cancellationToken);

            _logger.LogInformation(
                "Creating attribute definition from external sync: {ExternalId} - {Code} (Id: {Id})",
                lookup.EffectiveExternalId, request.Code, attribute.Id);
        }
        else
        {
            attribute = lookup.Entity!;

            // Update existing attribute
            attribute.UpdateFromExternal(
                name: request.Name,
                unit: request.Unit,
                isFilterable: request.IsFilterable,
                isRequired: request.IsRequired,
                isVisibleOnProductPage: request.IsVisibleOnProductPage,
                displayOrder: request.DisplayOrder,
                externalCode: request.ExternalCode);

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
