using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryTree;

/// <summary>
/// Query to get category tree (hierarchical structure)
/// </summary>
public record GetCategoryTreeQuery(bool ActiveOnly = true) : IQuery<Result<List<CategoryTreeDto>>>;
