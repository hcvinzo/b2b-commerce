using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Auth;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<LoginResponseDto>>;
