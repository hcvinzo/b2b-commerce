using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout a user (invalidate refresh token)
/// </summary>
public record LogoutCommand(Guid UserId) : ICommand<Result>;
