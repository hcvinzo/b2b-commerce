using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.CreateCollection;

/// <summary>
/// Command to create a new collection
/// </summary>
public record CreateCollectionCommand(
    string Name,
    string? Description,
    string? ImageUrl,
    CollectionType Type,
    int DisplayOrder,
    bool IsActive,
    bool IsFeatured,
    DateTime? StartDate,
    DateTime? EndDate) : ICommand<Result<CollectionDto>>;
