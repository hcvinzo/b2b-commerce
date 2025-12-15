using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.UpdateAttributeDefinition;

/// <summary>
/// Handler for UpdateAttributeDefinitionCommand
/// </summary>
public class UpdateAttributeDefinitionCommandHandler : ICommandHandler<UpdateAttributeDefinitionCommand, Result<AttributeDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAttributeDefinitionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AttributeDefinitionDto>> Handle(UpdateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(request.Id, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<AttributeDefinitionDto>.Failure($"Attribute definition with ID '{request.Id}' not found.");
        }

        // Update the attribute definition
        attributeDefinition.Update(
            request.Name,
            request.NameEn,
            request.Unit,
            request.IsFilterable,
            request.IsRequired,
            request.IsVisibleOnProductPage,
            request.DisplayOrder);

        _unitOfWork.AttributeDefinitions.Update(attributeDefinition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AttributeDefinitionDto>.Success(_mapper.Map<AttributeDefinitionDto>(attributeDefinition));
    }
}
