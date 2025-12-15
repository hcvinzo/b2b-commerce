using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypeByCode;

/// <summary>
/// Handler for GetProductTypeByCodeQuery
/// </summary>
public class GetProductTypeByCodeQueryHandler : IQueryHandler<GetProductTypeByCodeQuery, Result<ProductTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductTypeByCodeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductTypeDto>> Handle(GetProductTypeByCodeQuery request, CancellationToken cancellationToken)
    {
        var productType = await _unitOfWork.ProductTypes.GetByCodeAsync(request.Code, cancellationToken);
        if (productType == null)
        {
            return Result<ProductTypeDto>.Failure($"Product type with code '{request.Code}' not found.");
        }

        return Result<ProductTypeDto>.Success(_mapper.Map<ProductTypeDto>(productType));
    }
}
