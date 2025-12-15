using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetFilterableAttributes;

/// <summary>
/// Query to get all filterable attribute definitions (for product filters)
/// </summary>
public record GetFilterableAttributesQuery() : IQuery<Result<IEnumerable<AttributeDefinitionDto>>>;
