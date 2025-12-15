using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.ProductTypes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.ProductTypes.Queries.GetProductTypes;

/// <summary>
/// Handler for GetProductTypesQuery
/// </summary>
public class GetProductTypesQueryHandler : IQueryHandler<GetProductTypesQuery, Result<IEnumerable<ProductTypeListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductTypesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ProductTypeListDto>>> Handle(GetProductTypesQuery request, CancellationToken cancellationToken)
    {
        var (productTypes, _) = await _unitOfWork.ProductTypes.GetPagedAsync(1, int.MaxValue, request.IsActive, cancellationToken);
        var dtos = _mapper.Map<IEnumerable<ProductTypeListDto>>(productTypes);
        return Result<IEnumerable<ProductTypeListDto>>.Success(dtos);
    }
}
