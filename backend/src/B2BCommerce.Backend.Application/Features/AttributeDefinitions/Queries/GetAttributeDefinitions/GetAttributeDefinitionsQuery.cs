using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;

/// <summary>
/// Query to get all attribute definitions
/// </summary>
public record GetAttributeDefinitionsQuery() : IQuery<Result<IEnumerable<AttributeDefinitionDto>>>;
