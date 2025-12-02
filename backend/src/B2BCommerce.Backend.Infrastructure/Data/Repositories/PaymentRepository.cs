using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Payment repository implementation for payment-specific operations
/// </summary>
public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
