using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RotateApiKey;

/// <summary>
/// Command to rotate an API key (creates new key, revokes old)
/// </summary>
public record RotateApiKeyCommand(Guid Id, string CreatedBy) : ICommand<Result<CreateApiKeyResponseDto>>;
