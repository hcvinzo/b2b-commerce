using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.CreateAttributeDefinition;

/// <summary>
/// Handler for CreateAttributeDefinitionCommand
/// </summary>
public class CreateAttributeDefinitionCommandHandler : ICommandHandler<CreateAttributeDefinitionCommand, Result<AttributeDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAttributeDefinitionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AttributeDefinitionDto>> Handle(CreateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingAttribute = await _unitOfWork.AttributeDefinitions.GetByCodeAsync(request.Code, cancellationToken);
        if (existingAttribute != null)
        {
            return Result<AttributeDefinitionDto>.Failure($"An attribute definition with code '{request.Code}' already exists.");
        }

        // Create the attribute definition
        var attributeDefinition = AttributeDefinition.Create(
            request.Code,
            request.Name,
            request.Type,
            request.Unit,
            request.IsFilterable,
            request.IsRequired,
            request.IsVisibleOnProductPage,
            request.DisplayOrder);

        // Add predefined values if provided
        if (request.PredefinedValues != null)
        {
            foreach (var valueDto in request.PredefinedValues)
            {
                attributeDefinition.AddPredefinedValue(
                    valueDto.Value,
                    valueDto.DisplayText,
                    valueDto.DisplayOrder);
            }
        }

        await _unitOfWork.AttributeDefinitions.AddAsync(attributeDefinition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with predefined values
        var result = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(attributeDefinition.Id, cancellationToken);
        return Result<AttributeDefinitionDto>.Success(_mapper.Map<AttributeDefinitionDto>(result));
    }
}
