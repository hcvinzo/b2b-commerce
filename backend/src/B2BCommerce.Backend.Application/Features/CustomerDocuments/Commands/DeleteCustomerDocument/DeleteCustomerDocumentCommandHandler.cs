using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.DeleteCustomerDocument;

/// <summary>
/// Handler for DeleteCustomerDocumentCommand
/// </summary>
public class DeleteCustomerDocumentCommandHandler : ICommandHandler<DeleteCustomerDocumentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    public DeleteCustomerDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<Result<bool>> Handle(
        DeleteCustomerDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.CustomerDocuments.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.CustomerId != request.CustomerId)
        {
            return Result<bool>.Failure("Document not found", "DOCUMENT_NOT_FOUND");
        }

        // Delete the file from S3
        if (!string.IsNullOrEmpty(document.ContentUrl))
        {
            await _storageService.DeleteFileAsync(document.ContentUrl, cancellationToken);
        }

        // Soft delete the document record
        document.MarkAsDeleted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
