using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionById;

/// <summary>
/// Handler for GetAttributeDefinitionByIdQuery
/// </summary>
public class GetAttributeDefinitionByIdQueryHandler : IQueryHandler<GetAttributeDefinitionByIdQuery, Result<AttributeDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAttributeDefinitionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AttributeDefinitionDto>> Handle(GetAttributeDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(request.Id, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<AttributeDefinitionDto>.Failure($"Attribute definition with ID '{request.Id}' not found.");
        }

        return Result<AttributeDefinitionDto>.Success(_mapper.Map<AttributeDefinitionDto>(attributeDefinition));
    }
}
