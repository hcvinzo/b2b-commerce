using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.DeleteCollection;

/// <summary>
/// Command to delete (soft delete) a collection
/// </summary>
public record DeleteCollectionCommand(Guid Id) : ICommand<Result>;
