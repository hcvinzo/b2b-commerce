using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // Global soft delete filter
        builder.HasQueryFilter(o => !o.IsDeleted);

        // Properties
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        // Enum properties
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Money value objects - Subtotal
        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("SubtotalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("SubtotalCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Tax Amount
        builder.OwnsOne(o => o.TaxAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TaxAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Discount Amount
        builder.OwnsOne(o => o.DiscountAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DiscountAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("DiscountCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Shipping Cost
        builder.OwnsOne(o => o.ShippingCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ShippingCostAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ShippingCostCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Total Amount
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Exchange rate properties
        builder.Property(o => o.LockedExchangeRate)
            .HasColumnType("decimal(18,6)");

        builder.Property(o => o.ExchangeRateFrom)
            .HasMaxLength(3);

        builder.Property(o => o.ExchangeRateTo)
            .HasMaxLength(3);

        // Approval properties
        builder.Property(o => o.ApprovedAt);

        builder.Property(o => o.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(o => o.RejectionReason)
            .HasMaxLength(1000);

        // Address value object as owned type - Shipping Address
        builder.OwnsOne(o => o.ShippingAddress, address =>
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

        builder.Property(o => o.ShippingNote)
            .HasMaxLength(1000);

        builder.Property(o => o.EstimatedDeliveryDate);

        // Notes
        builder.Property(o => o.CustomerNote)
            .HasMaxLength(2000);

        builder.Property(o => o.InternalNote)
            .HasMaxLength(2000);

        // Audit properties
        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.CreatedBy)
            .HasMaxLength(100);

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.DeletedAt);

        builder.Property(o => o.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Shipment)
            .WithOne(s => s.Order)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(o => o.CustomerId);

        builder.HasIndex(o => o.Status);

        builder.HasIndex(o => o.ApprovalStatus);

        builder.HasIndex(o => o.CreatedAt);

        builder.HasIndex(o => o.IsDeleted);
    }
}
