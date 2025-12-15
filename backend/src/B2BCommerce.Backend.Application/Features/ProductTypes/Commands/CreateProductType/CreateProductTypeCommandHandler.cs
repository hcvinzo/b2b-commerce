using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.CreateProductType;

/// <summary>
/// Handler for CreateProductTypeCommand
/// </summary>
public class CreateProductTypeCommandHandler : ICommandHandler<CreateProductTypeCommand, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(CreateProductTypeCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingProductType = await _unitOfWork.ProductTypes.GetByCodeAsync(request.Code, cancellationToken);
        if (existingProductType != null)
        {
            return Result<ProductTypeDto>.Failure($"A product type with code '{request.Code}' already exists.");
        }

        // Create the product type
        var productType = ProductType.Create(
            request.Code,
            request.Name,
            request.Description);

        // Set active state if explicitly set to false
        if (!request.IsActive)
        {
            productType.Deactivate();
        }

        // Add attributes if provided
        if (request.Attributes != null)
        {
            foreach (var attributeDto in request.Attributes)
            {
                // Verify attribute definition exists
                var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attributeDto.AttributeDefinitionId, cancellationToken);
                if (attributeDefinition == null)
                {
                    return Result<ProductTypeDto>.Failure($"Attribute definition with ID '{attributeDto.AttributeDefinitionId}' not found.");
                }

                productType.AddAttribute(
                    attributeDto.AttributeDefinitionId,
                    attributeDto.IsRequired,
                    attributeDto.DisplayOrder);
            }
        }

        await _unitOfWork.ProductTypes.AddAsync(productType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with attributes
        var result = await _unitOfWork.ProductTypes.GetWithAttributesAsync(productType.Id, cancellationToken);
        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(result));
    }
}
