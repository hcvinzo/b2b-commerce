using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCreditLimit;

/// <summary>
/// Handler for UpdateCreditLimitCommand
/// </summary>
public class UpdateCreditLimitCommandHandler : ICommandHandler<UpdateCreditLimitCommand, Result<CustomerDto>>
{
    private readonly ICustomerService _customerService;

    public UpdateCreditLimitCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCreditLimitCommand request, CancellationToken cancellationToken)
    {
        return await _customerService.UpdateCreditLimitAsync(request.Id, request.NewCreditLimit, cancellationToken);
    }
}
