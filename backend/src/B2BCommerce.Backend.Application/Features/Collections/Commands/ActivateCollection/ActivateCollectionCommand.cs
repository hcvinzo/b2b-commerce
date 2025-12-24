using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.ActivateCollection;

/// <summary>
/// Command to activate a collection
/// </summary>
public record ActivateCollectionCommand(Guid Id) : ICommand<Result>;
