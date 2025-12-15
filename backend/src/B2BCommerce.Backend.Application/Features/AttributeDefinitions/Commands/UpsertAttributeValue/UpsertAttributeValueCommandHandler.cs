using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpsertAttributeValue;

/// <summary>
/// Handler for UpsertAttributeValueCommand.
/// Creates or updates a predefined value for an attribute definition.
/// </summary>
public class UpsertAttributeValueCommandHandler : ICommandHandler<UpsertAttributeValueCommand, Result<AttributeValueDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpsertAttributeValueCommandHandler> _logger;

    public UpsertAttributeValueCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpsertAttributeValueCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AttributeValueDto>> Handle(UpsertAttributeValueCommand request, CancellationToken cancellationToken)
    {
        // 1. Find attribute definition
        Domain.Entities.AttributeDefinition? attribute = null;

        if (request.AttributeDefinitionId.HasValue)
        {
            attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(
                request.AttributeDefinitionId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.AttributeExternalId))
        {
            attribute = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesByExternalIdAsync(
                request.AttributeExternalId, cancellationToken);
        }

        if (attribute == null)
        {
            return Result<AttributeValueDto>.Failure(
                "Attribute definition not found",
                "NOT_FOUND");
        }

        // 2. Validate attribute type allows predefined values
        if (!attribute.RequiresPredefinedValues())
        {
            return Result<AttributeValueDto>.Failure(
                $"Attribute '{attribute.Code}' is of type '{attribute.Type}' which does not support predefined values",
                "INVALID_TYPE");
        }

        // 3. Find or create value
        var existingValue = attribute.FindPredefinedValueByValue(request.Value);

        if (existingValue != null)
        {
            // Update existing
            attribute.UpdatePredefinedValue(existingValue.Id, request.Value, request.DisplayText, request.DisplayOrder);
            attribute.UpdatedBy = request.ModifiedBy;

            _logger.LogInformation(
                "Updated attribute value '{Value}' for attribute {AttributeCode}",
                request.Value, attribute.Code);
        }
        else
        {
            // Add new
            existingValue = attribute.AddPredefinedValue(request.Value, request.DisplayText, request.DisplayOrder);
            attribute.UpdatedBy = request.ModifiedBy;

            _logger.LogInformation(
                "Added attribute value '{Value}' to attribute {AttributeCode}",
                request.Value, attribute.Code);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AttributeValueDto>.Success(_mapper.Map<AttributeValueDto>(existingValue));
    }
}
