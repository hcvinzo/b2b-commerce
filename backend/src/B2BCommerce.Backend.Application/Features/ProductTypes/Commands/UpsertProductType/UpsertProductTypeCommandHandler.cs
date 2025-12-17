using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpsertProductType;

/// <summary>
/// Handler for UpsertProductTypeCommand.
/// Creates or updates a product type based on Id, ExternalId, or Code.
/// </summary>
public class UpsertProductTypeCommandHandler : ICommandHandler<UpsertProductTypeCommand, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpsertProductTypeCommandHandler> _logger;

    public UpsertProductTypeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpsertProductTypeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductTypeDto>> Handle(UpsertProductTypeCommand request, CancellationToken cancellationToken)
    {
        // 1. Find existing product type (by Id → ExternalId → Code)
        ProductType? productType = null;
        bool createWithSpecificId = false;

        if (request.Id.HasValue)
        {
            productType = await _unitOfWork.ProductTypes.GetWithAttributesAsync(request.Id.Value, cancellationToken);

            // If Id is provided but not found, we'll create with that specific Id
            if (productType == null)
            {
                createWithSpecificId = true;
            }
        }

        // If not found by Id, try ExternalId
        if (productType == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalId))
        {
            productType = await _unitOfWork.ProductTypes.GetWithAttributesByExternalIdAsync(request.ExternalId, cancellationToken);
        }

        // If still not found, try Code as fallback
        if (productType == null && !createWithSpecificId)
        {
            productType = await _unitOfWork.ProductTypes.GetByCodeWithAttributesAsync(request.Code, cancellationToken);
        }

        // 2. Create or Update
        if (productType == null)
        {
            // Validate ExternalId is provided for new creation (unless Id-based creation)
            var externalId = request.ExternalId;
            if (string.IsNullOrEmpty(externalId) && request.Id.HasValue)
            {
                externalId = request.Id.Value.ToString();
            }

            if (string.IsNullOrEmpty(externalId))
            {
                return Result<ProductTypeDto>.Failure(
                    "ExternalId or Id is required for creating new product type",
                    "EXTERNAL_ID_REQUIRED");
            }

            // Check if code already exists (case-insensitive)
            var existingByCode = await _unitOfWork.ProductTypes.GetByCodeAsync(request.Code, cancellationToken);
            if (existingByCode != null)
            {
                return Result<ProductTypeDto>.Failure(
                    $"A product type with code '{request.Code}' already exists",
                    "CODE_EXISTS");
            }

            productType = ProductType.CreateFromExternal(
                externalId: externalId,
                code: request.Code,
                name: request.Name,
                description: request.Description,
                isActive: request.IsActive,
                externalCode: request.ExternalCode,
                specificId: createWithSpecificId ? request.Id : null);

            productType.CreatedBy = request.ModifiedBy;

            // Add attributes if provided
            if (request.Attributes != null && request.Attributes.Count > 0)
            {
                var attributeResult = await AddAttributesToProductType(productType, request.Attributes, cancellationToken);
                if (!attributeResult.IsSuccess)
                {
                    return Result<ProductTypeDto>.Failure(attributeResult.ErrorMessage!, attributeResult.ErrorCode);
                }
            }

            await _unitOfWork.ProductTypes.AddAsync(productType, cancellationToken);

            _logger.LogInformation(
                "Creating product type from external sync: {ExternalId} - {Code} (Id: {Id})",
                externalId, request.Code, productType.Id);
        }
        else
        {
            // Update existing product type
            productType.UpdateFromExternal(
                name: request.Name,
                description: request.Description,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            productType.UpdatedBy = request.ModifiedBy;

            // Sync attributes (full replacement)
            if (request.Attributes != null)
            {
                var syncResult = await SyncAttributes(productType, request.Attributes, cancellationToken);
                if (!syncResult.IsSuccess)
                {
                    return Result<ProductTypeDto>.Failure(syncResult.ErrorMessage!, syncResult.ErrorCode);
                }
            }

            _logger.LogInformation(
                "Updating product type from external sync: {ExternalId} - {Code} (Id: {Id})",
                productType.ExternalId, request.Code, productType.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with attributes
        var result = await _unitOfWork.ProductTypes.GetWithAttributesAsync(productType.Id, cancellationToken);
        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(result));
    }

    /// <summary>
    /// Adds attributes to a new product type.
    /// </summary>
    private async Task<Result> AddAttributesToProductType(
        ProductType productType,
        List<UpsertProductTypeAttributeDto> attributes,
        CancellationToken cancellationToken)
    {
        foreach (var attrDto in attributes)
        {
            var attrDefId = await ResolveAttributeDefinitionId(attrDto, cancellationToken);
            if (!attrDefId.HasValue)
            {
                return Result.Failure(
                    $"Attribute definition not found: Id={attrDto.AttributeDefinitionId}, ExternalId={attrDto.AttributeExternalId}, Code={attrDto.AttributeCode}",
                    "ATTRIBUTE_NOT_FOUND");
            }

            var newAttribute = productType.AddAttribute(attrDefId.Value, attrDto.IsRequired, attrDto.DisplayOrder);

            // Explicitly add the new attribute to the DbContext for proper tracking
            await _unitOfWork.ProductTypes.AddProductTypeAttributeAsync(newAttribute, cancellationToken);
        }

        return Result.Success();
    }

    /// <summary>
    /// Syncs attributes using full replacement semantics.
    /// Attributes are matched by their AttributeDefinitionId.
    /// </summary>
    private async Task<Result> SyncAttributes(
        ProductType productType,
        List<UpsertProductTypeAttributeDto> newAttributes,
        CancellationToken cancellationToken)
    {
        // Build a set of attribute definition IDs from the new list
        var newAttrDefIds = new HashSet<Guid>();
        var attrDtoMap = new Dictionary<Guid, UpsertProductTypeAttributeDto>();

        foreach (var attrDto in newAttributes)
        {
            var attrDefId = await ResolveAttributeDefinitionId(attrDto, cancellationToken);
            if (!attrDefId.HasValue)
            {
                return Result.Failure(
                    $"Attribute definition not found: Id={attrDto.AttributeDefinitionId}, ExternalId={attrDto.AttributeExternalId}, Code={attrDto.AttributeCode}",
                    "ATTRIBUTE_NOT_FOUND");
            }

            newAttrDefIds.Add(attrDefId.Value);
            attrDtoMap[attrDefId.Value] = attrDto;
        }

        // Get existing attribute definition IDs
        var existingAttrDefIds = productType.Attributes.Select(a => a.AttributeDefinitionId).ToList();

        // Remove attributes that are not in the new list
        foreach (var existingAttrDefId in existingAttrDefIds)
        {
            if (!newAttrDefIds.Contains(existingAttrDefId))
            {
                productType.RemoveAttribute(existingAttrDefId);
            }
        }

        // Add or update attributes
        foreach (var (attrDefId, attrDto) in attrDtoMap)
        {
            var existing = productType.FindAttributeByDefinitionId(attrDefId);
            if (existing != null)
            {
                // Update existing
                productType.UpdateAttribute(attrDefId, attrDto.IsRequired, attrDto.DisplayOrder);
            }
            else
            {
                // Add new
                var newAttribute = productType.AddAttribute(attrDefId, attrDto.IsRequired, attrDto.DisplayOrder);

                // Explicitly add the new attribute to the DbContext for proper tracking
                await _unitOfWork.ProductTypes.AddProductTypeAttributeAsync(newAttribute, cancellationToken);
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Resolves attribute definition ID from various identifiers.
    /// Priority: Id → ExternalId → Code
    /// </summary>
    private async Task<Guid?> ResolveAttributeDefinitionId(UpsertProductTypeAttributeDto attrDto, CancellationToken cancellationToken)
    {
        // Try by Id first
        if (attrDto.AttributeDefinitionId.HasValue)
        {
            var attr = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attrDto.AttributeDefinitionId.Value, cancellationToken);
            if (attr != null)
            {
                return attr.Id;
            }
        }

        // Try by ExternalId
        if (!string.IsNullOrEmpty(attrDto.AttributeExternalId))
        {
            var attr = await _unitOfWork.AttributeDefinitions.GetByExternalIdAsync(attrDto.AttributeExternalId, cancellationToken);
            if (attr != null)
            {
                return attr.Id;
            }
        }

        // Try by Code
        if (!string.IsNullOrEmpty(attrDto.AttributeCode))
        {
            var attr = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(attrDto.AttributeCode, cancellationToken);
            if (attr != null)
            {
                return attr.Id;
            }
        }

        return null;
    }
}
