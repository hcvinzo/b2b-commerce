using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.DeactivateCollection;

/// <summary>
/// Handler for DeactivateCollectionCommand
/// </summary>
public class DeactivateCollectionCommandHandler : ICommandHandler<DeactivateCollectionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCollectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _unitOfWork.Collections.GetByIdAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        collection.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
