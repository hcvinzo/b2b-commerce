using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Products;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.SetProductRelations;

/// <summary>
/// Command to set related products for a specific relation type.
/// Replaces all existing relations of that type with the provided list.
/// Creates bidirectional relationships automatically.
/// </summary>
public record SetProductRelationsCommand(
    Guid ProductId,
    ProductRelationType RelationType,
    List<RelatedProductInputDto> RelatedProducts) : ICommand<Result>;
