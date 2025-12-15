using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        // Global soft delete filter
        builder.HasQueryFilter(oi => !oi.IsDeleted);

        // Properties
        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.ProductSKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // Money value objects - Unit Price
        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Subtotal
        builder.OwnsOne(oi => oi.Subtotal, money =>
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

        builder.Property(oi => oi.TaxRate)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        // Money value objects - Tax Amount
        builder.OwnsOne(oi => oi.TaxAmount, money =>
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
        builder.OwnsOne(oi => oi.DiscountAmount, money =>
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

        // Money value objects - Total Amount
        builder.OwnsOne(oi => oi.TotalAmount, money =>
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

        // Store SerialNumbers as JSON
        builder.Property(oi => oi.SerialNumbers)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");

        // Audit properties
        builder.Property(oi => oi.CreatedAt)
            .IsRequired();

        builder.Property(oi => oi.CreatedBy)
            .HasMaxLength(100);

        builder.Property(oi => oi.UpdatedAt);

        builder.Property(oi => oi.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(oi => oi.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(oi => oi.DeletedAt);

        builder.Property(oi => oi.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(oi => oi.OrderId);

        builder.HasIndex(oi => oi.ProductId);

        builder.HasIndex(oi => oi.ProductSKU);

        builder.HasIndex(oi => oi.IsDeleted);
    }
}
