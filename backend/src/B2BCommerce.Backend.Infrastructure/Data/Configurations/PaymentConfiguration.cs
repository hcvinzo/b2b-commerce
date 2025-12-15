using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        // Global soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Properties
        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.PaymentNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Enum properties
        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Money value object - Amount
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Payment gateway information
        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.GatewayResponse)
            .HasMaxLength(2000);

        builder.Property(p => p.PaidAt);

        // Bank transfer information
        builder.Property(p => p.BankReferenceNumber)
            .HasMaxLength(100);

        builder.Property(p => p.BankAccountInfo)
            .HasMaxLength(500);

        // Audit properties
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedAt);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(p => p.OrderId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.PaymentNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.TransactionId);

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.PaymentMethod);

        builder.HasIndex(p => p.PaidAt);

        builder.HasIndex(p => p.IsDeleted);
    }
}
