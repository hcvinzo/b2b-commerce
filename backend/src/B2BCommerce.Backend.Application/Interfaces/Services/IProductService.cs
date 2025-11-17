using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Products;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for product operations
/// </summary>
public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductListDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProductListDto>>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductListDto>>> SearchAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
