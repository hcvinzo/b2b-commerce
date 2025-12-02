using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LogoutAsync(request.UserId, cancellationToken);
    }
}
