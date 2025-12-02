using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.ActivateCustomer;

/// <summary>
/// Handler for ActivateCustomerCommand
/// </summary>
public class ActivateCustomerCommandHandler : ICommandHandler<ActivateCustomerCommand, Result>
{
    private readonly ICustomerService _customerService;

    public ActivateCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _customerService.ActivateAsync(request.Id, cancellationToken);
    }
}
