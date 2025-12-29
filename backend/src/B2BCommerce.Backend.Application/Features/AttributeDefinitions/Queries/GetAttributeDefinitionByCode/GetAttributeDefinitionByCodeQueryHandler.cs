using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionByCode;

/// <summary>
/// Handler for GetAttributeDefinitionByCodeQuery
/// </summary>
public class GetAttributeDefinitionByCodeQueryHandler : IQueryHandler<GetAttributeDefinitionByCodeQuery, Result<AttributeDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAttributeDefinitionByCodeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AttributeDefinitionDto>> Handle(GetAttributeDefinitionByCodeQuery request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(request.Code, cancellationToken);
        if (attributeDefinition is null)
        {
            return Result<AttributeDefinitionDto>.Failure($"Attribute definition with code '{request.Code}' not found.", "ATTRIBUTE_NOT_FOUND");
        }

        return Result<AttributeDefinitionDto>.Success(_mapper.Map<AttributeDefinitionDto>(attributeDefinition));
    }
}
