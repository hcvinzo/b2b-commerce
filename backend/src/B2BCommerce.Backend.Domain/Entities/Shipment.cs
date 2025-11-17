using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Shipment entity for order shipments
/// </summary>
public class Shipment : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string ShipmentNumber { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? CarrierName { get; private set; }
    public string? TrackingNumber { get; private set; }
    public Address ShippingAddress { get; private set; }

    public DateTime? ShippedDate { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }

    public string? ShippingNote { get; private set; }
    public string? DeliveryNote { get; private set; }

    // Dimensions and weight
    public decimal? TotalWeight { get; private set; }
    public int? PackageCount { get; private set; }

    // Navigation properties
    public Order? Order { get; set; }

    private Shipment() // For EF Core
    {
        ShipmentNumber = string.Empty;
        ShippingAddress = new Address("Street", "City", "State", "Country", "00000");
    }

    public Shipment(Guid orderId, Address shippingAddress, string? shippingNote = null)
    {
        OrderId = orderId;
        ShipmentNumber = GenerateShipmentNumber();
        Status = ShipmentStatus.Pending;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        ShippingNote = shippingNote;
    }

    private static string GenerateShipmentNumber()
    {
        return $"SHIP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    public void MarkAsPreparing()
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark shipment as preparing from status {Status}");

        Status = ShipmentStatus.Preparing;
    }

    public void MarkAsReadyToShip()
    {
        if (Status != ShipmentStatus.Preparing)
            throw new InvalidOperationException($"Cannot mark shipment as ready to ship from status {Status}");

        Status = ShipmentStatus.ReadyToShip;
    }

    public void Ship(string carrierName, string trackingNumber, DateTime? estimatedDeliveryDate = null)
    {
        if (Status != ShipmentStatus.ReadyToShip)
            throw new InvalidOperationException($"Cannot ship from status {Status}");

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name is required", nameof(carrierName));

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));

        Status = ShipmentStatus.Shipped;
        CarrierName = carrierName;
        TrackingNumber = trackingNumber;
        ShippedDate = DateTime.UtcNow;
        EstimatedDeliveryDate = estimatedDeliveryDate;
    }

    public void MarkAsInTransit()
    {
        if (Status != ShipmentStatus.Shipped)
            throw new InvalidOperationException($"Cannot mark as in transit from status {Status}");

        Status = ShipmentStatus.InTransit;
    }

    public void MarkAsOutForDelivery()
    {
        if (Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark as out for delivery from status {Status}");

        Status = ShipmentStatus.OutForDelivery;
    }

    public void MarkAsDelivered(string? deliveryNote = null)
    {
        if (Status != ShipmentStatus.OutForDelivery && Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark as delivered from status {Status}");

        Status = ShipmentStatus.Delivered;
        DeliveredDate = DateTime.UtcNow;
        DeliveryNote = deliveryNote;
    }

    public void Cancel()
    {
        if (Status == ShipmentStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered shipment");

        Status = ShipmentStatus.Cancelled;
    }

    public void MarkAsReturned()
    {
        Status = ShipmentStatus.Returned;
    }

    public void UpdateShippingDetails(decimal totalWeight, int packageCount)
    {
        TotalWeight = totalWeight;
        PackageCount = packageCount;
    }

    public void UpdateEstimatedDeliveryDate(DateTime estimatedDeliveryDate)
    {
        EstimatedDeliveryDate = estimatedDeliveryDate;
    }
}
