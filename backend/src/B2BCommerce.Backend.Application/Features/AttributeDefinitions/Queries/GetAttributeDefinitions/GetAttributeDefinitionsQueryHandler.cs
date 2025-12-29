using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

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
        IEnumerable<AttributeDefinition> attributeDefinitions;

        if (request.EntityType.HasValue)
        {
            // Filter by entity type
            attributeDefinitions = request.IncludeValues
                ? await _unitOfWork.AttributeDefinitions.GetByEntityTypeWithValuesAsync(request.EntityType.Value, cancellationToken)
                : await _unitOfWork.AttributeDefinitions.GetByEntityTypeAsync(request.EntityType.Value, cancellationToken);
        }
        else
        {
            // Get all
            attributeDefinitions = request.IncludeValues
                ? await _unitOfWork.AttributeDefinitions.GetAllWithValuesAsync(cancellationToken)
                : await _unitOfWork.AttributeDefinitions.GetAllAsync(cancellationToken);
        }

        var dtos = _mapper.Map<IEnumerable<AttributeDefinitionDto>>(attributeDefinitions);
        return Result<IEnumerable<AttributeDefinitionDto>>.Success(dtos);
    }
}
