using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetFilterableAttributes;

/// <summary>
/// Handler for GetFilterableAttributesQuery
/// </summary>
public class GetFilterableAttributesQueryHandler : IQueryHandler<GetFilterableAttributesQuery, Result<IEnumerable<AttributeDefinitionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFilterableAttributesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AttributeDefinitionDto>>> Handle(GetFilterableAttributesQuery request, CancellationToken cancellationToken)
    {
        var filterableAttributes = await _unitOfWork.AttributeDefinitions.GetFilterableAttributesAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<AttributeDefinitionDto>>(filterableAttributes);
        return Result<IEnumerable<AttributeDefinitionDto>>.Success(dtos);
    }
}
