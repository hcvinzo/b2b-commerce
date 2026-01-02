using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class DiscountRuleProductConfiguration : IEntityTypeConfiguration<DiscountRuleProduct>
{
    public void Configure(EntityTypeBuilder<DiscountRuleProduct> builder)
    {
        builder.ToTable("DiscountRuleProducts");

        builder.HasKey(p => p.Id);

        // Global soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Properties
        builder.Property(p => p.DiscountRuleId)
            .IsRequired();

        builder.Property(p => p.ProductId)
            .IsRequired();

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

        // Relationships
        builder.HasOne(p => p.DiscountRule)
            .WithMany(r => r.Products)
            .HasForeignKey(p => p.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.DiscountRuleId);

        builder.HasIndex(p => p.ProductId);

        // Composite unique index to prevent duplicates
        builder.HasIndex(p => new { p.DiscountRuleId, p.ProductId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.IsDeleted);
    }
}
