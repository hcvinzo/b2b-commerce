using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionById;

/// <summary>
/// Query to get an attribute definition by its ID
/// </summary>
public record GetAttributeDefinitionByIdQuery(Guid Id) : IQuery<Result<AttributeDefinitionDto>>;
