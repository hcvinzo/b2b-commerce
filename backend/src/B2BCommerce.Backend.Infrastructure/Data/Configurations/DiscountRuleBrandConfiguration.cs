using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class DiscountRuleBrandConfiguration : IEntityTypeConfiguration<DiscountRuleBrand>
{
    public void Configure(EntityTypeBuilder<DiscountRuleBrand> builder)
    {
        builder.ToTable("DiscountRuleBrands");

        builder.HasKey(b => b.Id);

        // Global soft delete filter
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Properties
        builder.Property(b => b.DiscountRuleId)
            .IsRequired();

        builder.Property(b => b.BrandId)
            .IsRequired();

        // Audit properties
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.UpdatedAt);

        builder.Property(b => b.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAt);

        builder.Property(b => b.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(b => b.DiscountRule)
            .WithMany(r => r.Brands)
            .HasForeignKey(b => b.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Brand)
            .WithMany()
            .HasForeignKey(b => b.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(b => b.DiscountRuleId);

        builder.HasIndex(b => b.BrandId);

        // Composite unique index to prevent duplicates
        builder.HasIndex(b => new { b.DiscountRuleId, b.BrandId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(b => b.IsDeleted);
    }
}
