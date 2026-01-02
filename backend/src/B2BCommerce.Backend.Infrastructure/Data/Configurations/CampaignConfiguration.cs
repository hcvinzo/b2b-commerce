using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasDefaultValue(CampaignStatus.Draft);

        builder.Property(c => c.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        // Money value objects as owned types - TotalBudgetLimit (optional)
        builder.OwnsOne(c => c.TotalBudgetLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalBudgetLimitAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("TotalBudgetLimitCurrency")
                .HasMaxLength(3);
        });

        builder.Property(c => c.TotalUsageLimit);

        // Money value objects - PerCustomerBudgetLimit (optional)
        builder.OwnsOne(c => c.PerCustomerBudgetLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PerCustomerBudgetLimitAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("PerCustomerBudgetLimitCurrency")
                .HasMaxLength(3);
        });

        builder.Property(c => c.PerCustomerUsageLimit);

        // Money value objects - TotalDiscountUsed (required)
        builder.OwnsOne(c => c.TotalDiscountUsed, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalDiscountUsedAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalDiscountUsedCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(c => c.TotalUsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        // External entity fields (from ExternalEntity base class)
        builder.Property(c => c.ExternalCode)
            .HasMaxLength(100);

        builder.Property(c => c.ExternalId)
            .HasMaxLength(100);

        builder.Property(c => c.LastSyncedAt);

        // Audit properties
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(c => c.DiscountRules)
            .WithOne(r => r.Campaign)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Usages)
            .WithOne(u => u.Campaign)
            .HasForeignKey(u => u.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.Status);

        builder.HasIndex(c => c.StartDate);

        builder.HasIndex(c => c.EndDate);

        // Composite index for active campaign queries
        builder.HasIndex(c => new { c.Status, c.StartDate, c.EndDate });

        builder.HasIndex(c => c.Priority);

        builder.HasIndex(c => c.IsDeleted);

        builder.HasIndex(c => c.Name);

        // External ID unique index (primary upsert key, non-deleted, non-null only)
        builder.HasIndex(c => c.ExternalId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");

        // External code index (optional field, non-unique for lookups)
        builder.HasIndex(c => c.ExternalCode)
            .HasFilter("\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

        builder.HasIndex(c => c.LastSyncedAt);
    }
}
