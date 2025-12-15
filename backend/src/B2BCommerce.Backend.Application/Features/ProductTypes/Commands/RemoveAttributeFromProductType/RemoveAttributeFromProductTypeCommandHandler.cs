using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.RemoveAttributeFromProductType;

/// <summary>
/// Handler for RemoveAttributeFromProductTypeCommand
/// </summary>
public class RemoveAttributeFromProductTypeCommandHandler : ICommandHandler<RemoveAttributeFromProductTypeCommand, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RemoveAttributeFromProductTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(RemoveAttributeFromProductTypeCommand request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesAsync(request.ProductTypeId, cancellationToken);
        if (productType == null)
        {
            return Result<ProductTypeDto>.Failure($"Product type with ID '{request.ProductTypeId}' not found.");
        }

        // Check if attribute is assigned
        var existingAttribute = productType.Attributes.FirstOrDefault(a => a.AttributeDefinitionId == request.AttributeDefinitionId);
        if (existingAttribute == null)
        {
            return Result<ProductTypeDto>.Failure($"Attribute definition with ID '{request.AttributeDefinitionId}' is not assigned to this product type.");
        }

        // Remove the attribute
        productType.RemoveAttribute(request.AttributeDefinitionId);

        _unitOfWork.ProductTypes.Update(productType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with updated attributes
        var result = await _unitOfWork.ProductTypes.GetWithAttributesAsync(productType.Id, cancellationToken);
        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(result));
    }
}
