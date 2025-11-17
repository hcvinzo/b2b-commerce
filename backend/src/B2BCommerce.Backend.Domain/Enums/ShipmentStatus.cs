namespace B2BCommerce.Backend.Domain.Enums;

public enum ShipmentStatus
{
    Pending = 0,
    Preparing = 1,
    ReadyToShip = 2,
    Shipped = 3,
    InTransit = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7,
    Returned = 8
}
