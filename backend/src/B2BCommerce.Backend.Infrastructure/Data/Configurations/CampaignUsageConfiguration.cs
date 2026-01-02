using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CampaignUsageConfiguration : IEntityTypeConfiguration<CampaignUsage>
{
    public void Configure(EntityTypeBuilder<CampaignUsage> builder)
    {
        builder.ToTable("CampaignUsages");

        builder.HasKey(u => u.Id);

        // Global soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Properties
        builder.Property(u => u.CampaignId)
            .IsRequired();

        builder.Property(u => u.CustomerId)
            .IsRequired();

        builder.Property(u => u.OrderId)
            .IsRequired();

        builder.Property(u => u.OrderItemId);

        // Money value object - DiscountAmount
        builder.OwnsOne(u => u.DiscountAmount, money =>
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

        builder.Property(u => u.UsedAt)
            .IsRequired();

        builder.Property(u => u.IsReversed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.ReversedAt);

        // Audit properties
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100);

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt);

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(u => u.Campaign)
            .WithMany(c => c.Usages)
            .HasForeignKey(u => u.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Customer)
            .WithMany()
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Order)
            .WithMany()
            .HasForeignKey(u => u.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.OrderItem)
            .WithMany()
            .HasForeignKey(u => u.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(u => u.CampaignId);

        builder.HasIndex(u => u.CustomerId);

        builder.HasIndex(u => u.OrderId);

        builder.HasIndex(u => u.OrderItemId);

        // Composite index for per-customer queries
        builder.HasIndex(u => new { u.CampaignId, u.CustomerId });

        builder.HasIndex(u => u.UsedAt);

        builder.HasIndex(u => u.IsReversed);

        builder.HasIndex(u => u.IsDeleted);
    }
}
