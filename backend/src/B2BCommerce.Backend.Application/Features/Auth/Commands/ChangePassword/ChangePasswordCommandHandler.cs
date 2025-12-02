using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Auth;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        return await _authService.ChangePasswordAsync(request.UserId, changePasswordDto, cancellationToken);
    }
}
