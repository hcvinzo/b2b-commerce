using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetSubCategories;

/// <summary>
/// Query to get subcategories of a parent category
/// </summary>
public record GetSubCategoriesQuery(Guid ParentId, bool ActiveOnly = true) : IQuery<Result<List<CategoryListDto>>>;
