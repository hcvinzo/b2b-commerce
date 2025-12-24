using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocument;

/// <summary>
/// Handler for GetCustomerDocumentQuery
/// </summary>
public class GetCustomerDocumentQueryHandler : IQueryHandler<GetCustomerDocumentQuery, Result<CustomerDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerDocumentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDocumentDto>> Handle(
        GetCustomerDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.CustomerDocuments.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document is null || document.CustomerId != request.CustomerId)
        {
            return Result<CustomerDocumentDto>.Failure("Document not found", "DOCUMENT_NOT_FOUND");
        }

        var dto = _mapper.Map<CustomerDocumentDto>(document);

        return Result<CustomerDocumentDto>.Success(dto);
    }
}
