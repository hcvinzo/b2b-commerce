using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.UpdateCustomerDocument;

/// <summary>
/// Handler for UpdateCustomerDocumentCommand
/// </summary>
public class UpdateCustomerDocumentCommandHandler : ICommandHandler<UpdateCustomerDocumentCommand, Result<CustomerDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerDocumentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDocumentDto>> Handle(
        UpdateCustomerDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.CustomerDocuments.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.CustomerId != request.CustomerId)
        {
            return Result<CustomerDocumentDto>.Failure("Document not found", "DOCUMENT_NOT_FOUND");
        }

        document.Update(
            request.FileName,
            request.FileType,
            request.ContentUrl,
            request.FileSize);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CustomerDocumentDto>(document);

        return Result<CustomerDocumentDto>.Success(dto);
    }
}
