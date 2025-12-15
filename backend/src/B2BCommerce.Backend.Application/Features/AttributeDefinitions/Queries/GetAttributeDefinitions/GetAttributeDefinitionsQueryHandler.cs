using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;

/// <summary>
/// Handler for GetAttributeDefinitionsQuery
/// </summary>
public class GetAttributeDefinitionsQueryHandler : IQueryHandler<GetAttributeDefinitionsQuery, Result<IEnumerable<AttributeDefinitionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAttributeDefinitionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AttributeDefinitionDto>>> Handle(GetAttributeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var attributeDefinitions = await _unitOfWork.AttributeDefinitions.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<AttributeDefinitionDto>>(attributeDefinitions);
        return Result<IEnumerable<AttributeDefinitionDto>>.Success(dtos);
    }
}
