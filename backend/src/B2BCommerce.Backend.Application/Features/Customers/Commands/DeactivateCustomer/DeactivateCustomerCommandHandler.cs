using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.DeactivateCustomer;

/// <summary>
/// Handler for DeactivateCustomerCommand
/// </summary>
public class DeactivateCustomerCommandHandler : ICommandHandler<DeactivateCustomerCommand, Result>
{
    private readonly ICustomerService _customerService;

    public DeactivateCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _customerService.DeactivateAsync(request.Id, cancellationToken);
    }
}
