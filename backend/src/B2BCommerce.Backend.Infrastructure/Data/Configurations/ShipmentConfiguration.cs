using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.OrderId)
            .IsRequired();

        builder.Property(s => s.ShipmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Enum property
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.CarrierName)
            .HasMaxLength(200);

        builder.Property(s => s.TrackingNumber)
            .HasMaxLength(100);

        // Address value object as owned type
        builder.OwnsOne(s => s.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("ShippingState")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        // Dates
        builder.Property(s => s.ShippedDate);

        builder.Property(s => s.EstimatedDeliveryDate);

        builder.Property(s => s.DeliveredDate);

        // Notes
        builder.Property(s => s.ShippingNote)
            .HasMaxLength(2000);

        builder.Property(s => s.DeliveryNote)
            .HasMaxLength(2000);

        // Dimensions and weight
        builder.Property(s => s.TotalWeight)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.PackageCount);

        // Audit properties
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.DeletedAt);

        builder.Property(s => s.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(s => s.OrderId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(s => s.ShipmentNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(s => s.TrackingNumber);

        builder.HasIndex(s => s.Status);

        builder.HasIndex(s => s.ShippedDate);

        builder.HasIndex(s => s.DeliveredDate);

        builder.HasIndex(s => s.IsDeleted);
    }
}
