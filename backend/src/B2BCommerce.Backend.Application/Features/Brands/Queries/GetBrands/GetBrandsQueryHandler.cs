using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Brands;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Brands.Queries.GetBrands;

/// <summary>
/// Handler for GetBrandsQuery
/// </summary>
public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, Result<PagedResult<BrandListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<BrandListDto>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await _unitOfWork.Brands.GetAllAsync(cancellationToken);

        // Apply search filter
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchLower = request.Search.ToLower();
            brands = brands.Where(b =>
                b.Name.ToLower().Contains(searchLower) ||
                (b.Description != null && b.Description.ToLower().Contains(searchLower)));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            brands = brands.Where(b => b.IsActive == request.IsActive.Value);
        }

        // Apply sorting
        var sortedBrands = (request.SortBy.ToLower(), request.SortDirection.ToLower()) switch
        {
            ("name", "desc") => brands.OrderByDescending(b => b.Name),
            ("name", _) => brands.OrderBy(b => b.Name),
            ("createdat", "desc") => brands.OrderByDescending(b => b.CreatedAt),
            ("createdat", _) => brands.OrderBy(b => b.CreatedAt),
            ("isactive", "desc") => brands.OrderByDescending(b => b.IsActive),
            ("isactive", _) => brands.OrderBy(b => b.IsActive),
            _ => brands.OrderBy(b => b.Name)
        };

        var totalCount = sortedBrands.Count();
        var pagedBrands = sortedBrands
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BrandListDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive,
                ProductCount = b.Products?.Count ?? 0,
                CreatedAt = b.CreatedAt
            })
            .ToList();

        var result = new PagedResult<BrandListDto>(pagedBrands, request.PageNumber, request.PageSize, totalCount);
        return Result<PagedResult<BrandListDto>>.Success(result);
    }
}
