using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.DeleteCollection;

/// <summary>
/// Handler for DeleteCollectionCommand
/// </summary>
public class DeleteCollectionCommandHandler : ICommandHandler<DeleteCollectionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCollectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        // Get collection with products
        var collection = await _unitOfWork.Collections.GetWithProductsAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        // Soft delete the collection (this will also mark ProductCollections as deleted via ClearProducts)
        collection.ClearProducts();
        collection.MarkAsDeleted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
