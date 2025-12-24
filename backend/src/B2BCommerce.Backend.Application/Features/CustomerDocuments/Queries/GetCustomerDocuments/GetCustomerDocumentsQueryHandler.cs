using AutoMapper;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocuments;

/// <summary>
/// Handler for GetCustomerDocumentsQuery
/// </summary>
public class GetCustomerDocumentsQueryHandler : IQueryHandler<GetCustomerDocumentsQuery, Result<IEnumerable<CustomerDocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerDocumentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CustomerDocumentDto>>> Handle(
        GetCustomerDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var documents = await _unitOfWork.CustomerDocuments
            .GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        var dtos = _mapper.Map<IEnumerable<CustomerDocumentDto>>(documents);

        return Result<IEnumerable<CustomerDocumentDto>>.Success(dtos);
    }
}
