using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.AddAttributeToProductType;

/// <summary>
/// Handler for AddAttributeToProductTypeCommand
/// </summary>
public class AddAttributeToProductTypeCommandHandler : ICommandHandler<AddAttributeToProductTypeCommand, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddAttributeToProductTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(AddAttributeToProductTypeCommand request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesAsync(request.ProductTypeId, cancellationToken);
        if (productType == null)
        {
            return Result<ProductTypeDto>.Failure($"Product type with ID '{request.ProductTypeId}' not found.");
        }

        // Verify attribute definition exists
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(request.AttributeDefinitionId, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<ProductTypeDto>.Failure($"Attribute definition with ID '{request.AttributeDefinitionId}' not found.");
        }

        // Check if attribute is already assigned
        var existingAttribute = productType.Attributes.FirstOrDefault(a => a.AttributeDefinitionId == request.AttributeDefinitionId);
        if (existingAttribute != null)
        {
            return Result<ProductTypeDto>.Failure($"Attribute '{attributeDefinition.Name}' is already assigned to this product type.");
        }

        // Add the attribute
        productType.AddAttribute(
            request.AttributeDefinitionId,
            request.IsRequired,
            request.DisplayOrder);

        _unitOfWork.ProductTypes.Update(productType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with updated attributes
        var result = await _unitOfWork.ProductTypes.GetWithAttributesAsync(productType.Id, cancellationToken);
        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(result));
    }
}
