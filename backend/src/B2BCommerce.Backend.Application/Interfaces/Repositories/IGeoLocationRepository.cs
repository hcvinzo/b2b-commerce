using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

public interface IGeoLocationRepository : IGenericRepository<GeoLocation>
{
    Task<GeoLocation?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<GeoLocation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<GeoLocation>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GeoLocation>> GetByTypeIdAsync(Guid typeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GeoLocation>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GeoLocation>> GetRootLocationsAsync(CancellationToken cancellationToken = default);
}
