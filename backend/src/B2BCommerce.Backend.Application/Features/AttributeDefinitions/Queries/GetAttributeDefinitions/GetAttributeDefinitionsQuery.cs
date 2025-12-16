using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;

/// <summary>
/// Query to get all attribute definitions
/// </summary>
/// <param name="IncludeValues">If true, includes predefined values in the response (default: false)</param>
public record GetAttributeDefinitionsQuery(bool IncludeValues = false) : IQuery<Result<IEnumerable<AttributeDefinitionDto>>>;
