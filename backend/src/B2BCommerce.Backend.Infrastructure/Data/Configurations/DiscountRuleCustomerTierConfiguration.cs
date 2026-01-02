using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class DiscountRuleCustomerTierConfiguration : IEntityTypeConfiguration<DiscountRuleCustomerTier>
{
    public void Configure(EntityTypeBuilder<DiscountRuleCustomerTier> builder)
    {
        builder.ToTable("DiscountRuleCustomerTiers");

        builder.HasKey(t => t.Id);

        // Global soft delete filter
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Properties
        builder.Property(t => t.DiscountRuleId)
            .IsRequired();

        builder.Property(t => t.PriceTier)
            .IsRequired();

        // Audit properties
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(t => t.DiscountRule)
            .WithMany(r => r.CustomerTiers)
            .HasForeignKey(t => t.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.DiscountRuleId);

        builder.HasIndex(t => t.PriceTier);

        // Composite unique index to prevent duplicates
        builder.HasIndex(t => new { t.DiscountRuleId, t.PriceTier })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(t => t.IsDeleted);
    }
}
