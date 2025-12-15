using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Commands.DeleteAttributeDefinition;

/// <summary>
/// Handler for DeleteAttributeDefinitionCommand
/// </summary>
public class DeleteAttributeDefinitionCommandHandler : ICommandHandler<DeleteAttributeDefinitionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAttributeDefinitionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(request.Id, cancellationToken);
        if (attributeDefinition == null)
        {
            return Result<bool>.Failure($"Attribute definition with ID '{request.Id}' not found.");
        }

        // Soft delete
        attributeDefinition.MarkAsDeleted();
        _unitOfWork.AttributeDefinitions.Update(attributeDefinition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
