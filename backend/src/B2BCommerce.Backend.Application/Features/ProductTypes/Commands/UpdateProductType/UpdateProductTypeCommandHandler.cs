using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Commands.UpdateProductType;

/// <summary>
/// Handler for UpdateProductTypeCommand
/// </summary>
public class UpdateProductTypeCommandHandler : ICommandHandler<UpdateProductTypeCommand, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(UpdateProductTypeCommand request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesAsync(request.Id, cancellationToken);
        if (productType == null)
        {
            return Result<ProductTypeDto>.Failure($"Product type with ID '{request.Id}' not found.");
        }

        // Update the product type
        productType.Update(request.Name, request.Description);

        if (request.IsActive)
        {
            productType.Activate();
        }
        else
        {
            productType.Deactivate();
        }

        _unitOfWork.ProductTypes.Update(productType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(productType));
    }
}
