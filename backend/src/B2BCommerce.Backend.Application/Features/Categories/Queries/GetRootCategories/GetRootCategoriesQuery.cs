using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetRootCategories;

/// <summary>
/// Query to get root categories (categories without parent)
/// </summary>
public record GetRootCategoriesQuery(bool ActiveOnly = true) : IQuery<Result<List<CategoryListDto>>>;
