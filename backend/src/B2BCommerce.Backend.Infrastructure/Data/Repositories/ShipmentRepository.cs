using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Shipment repository implementation for shipment-specific operations
/// </summary>
public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
