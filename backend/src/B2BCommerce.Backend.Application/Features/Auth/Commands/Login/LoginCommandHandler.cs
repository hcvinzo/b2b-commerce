using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Auth;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand - delegates to IAuthService for Identity operations
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}
