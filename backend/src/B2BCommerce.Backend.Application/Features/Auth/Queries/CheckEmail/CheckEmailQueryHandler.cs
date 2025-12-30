using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Queries.CheckEmail;

/// <summary>
/// Handler for CheckEmailQuery
/// </summary>
public class CheckEmailQueryHandler : IQueryHandler<CheckEmailQuery, Result<bool>>
{
    private readonly IAuthService _authService;

    public CheckEmailQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        return await _authService.IsEmailAvailableAsync(request.Email, cancellationToken);
    }
}
