using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeById;

/// <summary>
/// Handler for GetProductTypeByIdQuery
/// </summary>
public class GetProductTypeByIdQueryHandler : IQueryHandler<GetProductTypeByIdQuery, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductTypeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(GetProductTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetWithAttributesAsync(request.Id, cancellationToken);
        if (productType == null)
        {
            return Result<ProductTypeDto>.Failure($"Product type with ID '{request.Id}' not found.");
        }

        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(productType));
    }
}
