using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionProducts;

/// <summary>
/// Command to set products in a manual collection.
/// Replaces all existing products with the provided list.
/// </summary>
public record SetCollectionProductsCommand(
    Guid CollectionId,
    List<ProductInCollectionInputDto> Products) : ICommand<Result>;
