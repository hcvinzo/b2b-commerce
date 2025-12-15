using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryById;

/// <summary>
/// Query to get a category by its ID
/// </summary>
public record GetCategoryByIdQuery(Guid Id) : IQuery<Result<CategoryDto>>;
