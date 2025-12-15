using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.AddAttributeValue;

/// <summary>
/// Handler for AddAttributeValueCommand
/// </summary>
public class AddAttributeValueCommandHandler : ICommandHandler<AddAttributeValueCommand, Result<AttributeValueDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddAttributeValueCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AttributeValueDto>> Handle(AddAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(request.AttributeDefinitionId, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<AttributeValueDto>.Failure($"Attribute definition with ID '{request.AttributeDefinitionId}' not found.");
        }

        // Check if value already exists
        var existingValue = attributeDefinition.PredefinedValues.FirstOrDefault(v => v.Value == request.Value);
        if (existingValue != null)
        {
            return Result<AttributeValueDto>.Failure($"A predefined value '{request.Value}' already exists for this attribute.");
        }

        // Add the predefined value
        var attributeValue = attributeDefinition.AddPredefinedValue(
            request.Value,
            request.DisplayText,
            request.DisplayOrder);

        _unitOfWork.AttributeDefinitions.Update(attributeDefinition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AttributeValueDto>.Success(_mapper.Map<AttributeValueDto>(attributeValue));
    }
}
