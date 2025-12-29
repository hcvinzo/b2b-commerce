using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetChildAttributes;

/// <summary>
/// Handler for GetChildAttributesQuery
/// </summary>
public class GetChildAttributesQueryHandler : IQueryHandler<GetChildAttributesQuery, Result<IEnumerable<AttributeDefinitionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetChildAttributesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AttributeDefinitionDto>>> Handle(GetChildAttributesQuery request, CancellationToken cancellationToken)
    {
        // Verify parent exists and is a Composite type
        var parent = await _unitOfWork.AttributeDefinitions.GetByIdAsync(request.ParentId, cancellationToken);
        if (parent is null)
        {
            return Result<IEnumerable<AttributeDefinitionDto>>.Failure("Parent attribute not found", "PARENT_NOT_FOUND");
        }

        if (parent.Type != Domain.Enums.AttributeType.Composite)
        {
            return Result<IEnumerable<AttributeDefinitionDto>>.Failure("Parent attribute is not a Composite type", "NOT_COMPOSITE");
        }

        var children = await _unitOfWork.AttributeDefinitions.GetByParentIdAsync(request.ParentId, cancellationToken);
        var dtos = _mapper.Map<IEnumerable<AttributeDefinitionDto>>(children);
        return Result<IEnumerable<AttributeDefinitionDto>>.Success(dtos);
    }
}
