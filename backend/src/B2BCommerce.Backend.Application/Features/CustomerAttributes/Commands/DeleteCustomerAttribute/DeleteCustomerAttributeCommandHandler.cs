using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.DeleteCustomerAttribute;

/// <summary>
/// Handler for DeleteCustomerAttributeCommand
/// </summary>
public class DeleteCustomerAttributeCommandHandler : ICommandHandler<DeleteCustomerAttributeCommand, Result>
{
    private readonly ICustomerAttributeService _customerAttributeService;

    public DeleteCustomerAttributeCommandHandler(ICustomerAttributeService customerAttributeService)
    {
        _customerAttributeService = customerAttributeService;
    }

    public async Task<Result> Handle(
        DeleteCustomerAttributeCommand request,
        CancellationToken cancellationToken)
    {
        return await _customerAttributeService.DeleteAsync(request.Id, cancellationToken);
    }
}
