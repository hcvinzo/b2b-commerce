using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.CreateCustomerDocument;

/// <summary>
/// Handler for CreateCustomerDocumentCommand
/// </summary>
public class CreateCustomerDocumentCommandHandler : ICommandHandler<CreateCustomerDocumentCommand, Result<CustomerDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerDocumentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDocumentDto>> Handle(
        CreateCustomerDocumentCommand request,
        CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDocumentDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        // Parse document type
        if (!Enum.TryParse<CustomerDocumentType>(request.DocumentType, out var documentType))
        {
            return Result<CustomerDocumentDto>.Failure("Invalid document type", "INVALID_DOCUMENT_TYPE");
        }

        // Check if document of this type already exists (replace if exists)
        var existingDocument = await _unitOfWork.CustomerDocuments
            .GetByCustomerIdAndTypeAsync(request.CustomerId, documentType, cancellationToken);

        if (existingDocument is not null)
        {
            // Update existing document
            existingDocument.Update(
                request.FileName,
                request.FileType,
                request.ContentUrl,
                request.FileSize);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedDto = _mapper.Map<CustomerDocumentDto>(existingDocument);
            return Result<CustomerDocumentDto>.Success(updatedDto);
        }

        // Create new document
        var document = CustomerDocument.Create(
            request.CustomerId,
            documentType,
            request.FileName,
            request.FileType,
            request.ContentUrl,
            request.FileSize);

        await _unitOfWork.CustomerDocuments.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CustomerDocumentDto>(document);

        return Result<CustomerDocumentDto>.Success(dto);
    }
}
