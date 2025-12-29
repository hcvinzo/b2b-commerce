using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitions;

/// <summary>
/// Query to get all attribute definitions
/// </summary>
/// <param name="IncludeValues">If true, includes predefined values in the response (default: false)</param>
/// <param name="EntityType">Optional filter by entity type (Product or Customer)</param>
public record GetAttributeDefinitionsQuery(
    bool IncludeValues = false,
    AttributeEntityType? EntityType = null) : IQuery<Result<IEnumerable<AttributeDefinitionDto>>>;
