using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.Features.AttributeDefinitions.Queries.GetAttributeDefinitionByCode;

/// <summary>
/// Query to get an attribute definition by its code
/// </summary>
public record GetAttributeDefinitionByCodeQuery(string Code) : IQuery<Result<AttributeDefinitionDto>>;
