using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Auth;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user
/// </summary>
public record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponseDto>>;
