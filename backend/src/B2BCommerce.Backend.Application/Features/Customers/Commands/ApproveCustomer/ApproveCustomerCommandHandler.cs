using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.ApproveCustomer;

/// <summary>
/// Handler for ApproveCustomerCommand
/// </summary>
public class ApproveCustomerCommandHandler : ICommandHandler<ApproveCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerService _customerService;

    public ApproveCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<CustomerDto>> Handle(ApproveCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _customerService.ApproveAsync(request.Id, request.ApprovedBy, cancellationToken);
    }
}
