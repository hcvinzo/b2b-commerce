using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

public interface ICustomerAddressRepository : IGenericRepository<CustomerAddress>
{
    Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerAddress?> GetDefaultAddressAsync(Guid customerId, CustomerAddressType addressType, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerAddress>> GetByAddressTypeAsync(Guid customerId, CustomerAddressType addressType, CancellationToken cancellationToken = default);
}
