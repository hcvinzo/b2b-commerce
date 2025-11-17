using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Payment repository interface for payment-specific operations
/// </summary>
public interface IPaymentRepository : IGenericRepository<Payment>
{
}
