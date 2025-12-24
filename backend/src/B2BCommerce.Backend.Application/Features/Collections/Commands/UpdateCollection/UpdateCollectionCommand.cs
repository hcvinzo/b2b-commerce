using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.UpdateCollection;

/// <summary>
/// Command to update an existing collection (type is immutable)
/// </summary>
public record UpdateCollectionCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    bool IsFeatured,
    DateTime? StartDate,
    DateTime? EndDate) : ICommand<Result<CollectionDto>>;
