using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Auth;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = request.RefreshToken
        };

        return await _authService.RefreshTokenAsync(refreshTokenDto, cancellationToken);
    }
}
