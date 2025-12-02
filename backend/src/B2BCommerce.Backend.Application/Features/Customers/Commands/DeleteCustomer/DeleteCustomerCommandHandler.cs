using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Handler for DeleteCustomerCommand
/// </summary>
public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand, Result>
{
    private readonly ICustomerService _customerService;

    public DeleteCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _customerService.DeleteAsync(request.Id, cancellationToken);
    }
}
