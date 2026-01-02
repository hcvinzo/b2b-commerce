using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.ToTable("DiscountRules");

        builder.HasKey(r => r.Id);

        // Global soft delete filter
        builder.HasQueryFilter(r => !r.IsDeleted);

        // Properties
        builder.Property(r => r.CampaignId)
            .IsRequired();

        builder.Property(r => r.DiscountType)
            .IsRequired();

        builder.Property(r => r.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.MaxDiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.ProductTargetType)
            .IsRequired();

        builder.Property(r => r.CustomerTargetType)
            .IsRequired();

        builder.Property(r => r.MinOrderAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.MinQuantity);

        // Audit properties
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.DeletedAt);

        builder.Property(r => r.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(r => r.Campaign)
            .WithMany(c => c.DiscountRules)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Products)
            .WithOne(p => p.DiscountRule)
            .HasForeignKey(p => p.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Categories)
            .WithOne(c => c.DiscountRule)
            .HasForeignKey(c => c.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Brands)
            .WithOne(b => b.DiscountRule)
            .HasForeignKey(b => b.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Customers)
            .WithOne(c => c.DiscountRule)
            .HasForeignKey(c => c.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.CustomerTiers)
            .WithOne(t => t.DiscountRule)
            .HasForeignKey(t => t.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.CampaignId);

        builder.HasIndex(r => r.DiscountType);

        builder.HasIndex(r => r.ProductTargetType);

        builder.HasIndex(r => r.CustomerTargetType);

        builder.HasIndex(r => r.IsDeleted);
    }
}
