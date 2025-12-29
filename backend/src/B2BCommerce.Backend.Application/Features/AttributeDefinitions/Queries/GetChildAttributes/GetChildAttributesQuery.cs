using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetChildAttributes;

/// <summary>
/// Query to get child attributes for a composite parent attribute
/// </summary>
/// <param name="ParentId">The parent attribute definition ID</param>
public record GetChildAttributesQuery(Guid ParentId) : IQuery<Result<IEnumerable<AttributeDefinitionDto>>>;
