using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

public interface ICustomerContactRepository : IGenericRepository<CustomerContact>
{
    Task<IEnumerable<CustomerContact>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerContact?> GetPrimaryContactAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerContact?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
