using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.DeactivateCollection;

/// <summary>
/// Command to deactivate a collection
/// </summary>
public record DeactivateCollectionCommand(Guid Id) : ICommand<Result>;
