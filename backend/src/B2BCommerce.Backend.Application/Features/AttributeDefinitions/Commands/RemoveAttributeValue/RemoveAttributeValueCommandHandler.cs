using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.RemoveAttributeValue;

/// <summary>
/// Handler for RemoveAttributeValueCommand
/// </summary>
public class RemoveAttributeValueCommandHandler : ICommandHandler<RemoveAttributeValueCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAttributeValueCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetWithPredefinedValuesAsync(request.AttributeDefinitionId, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<bool>.Failure($"Attribute definition with ID '{request.AttributeDefinitionId}' not found.");
        }

        // Find and remove the predefined value
        var attributeValue = attributeDefinition.PredefinedValues.FirstOrDefault(v => v.Id == request.AttributeValueId);
        if (attributeValue == null)
        {
            return Result<bool>.Failure($"Attribute value with ID '{request.AttributeValueId}' not found.");
        }

        attributeDefinition.RemovePredefinedValue(request.AttributeValueId);
        _unitOfWork.AttributeDefinitions.Update(attributeDefinition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
