using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Auth.Queries.CheckEmail;

/// <summary>
/// Query to check if an email is available for registration
/// </summary>
public record CheckEmailQuery(string Email) : IQuery<Result<bool>>;
