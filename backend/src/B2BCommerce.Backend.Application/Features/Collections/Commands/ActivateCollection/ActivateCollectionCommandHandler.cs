using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.ActivateCollection;

/// <summary>
/// Handler for ActivateCollectionCommand
/// </summary>
public class ActivateCollectionCommandHandler : ICommandHandler<ActivateCollectionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCollectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _unitOfWork.Collections.GetByIdAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        collection.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
