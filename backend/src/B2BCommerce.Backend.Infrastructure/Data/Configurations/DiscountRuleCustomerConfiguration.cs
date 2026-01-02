using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class DiscountRuleCustomerConfiguration : IEntityTypeConfiguration<DiscountRuleCustomer>
{
    public void Configure(EntityTypeBuilder<DiscountRuleCustomer> builder)
    {
        builder.ToTable("DiscountRuleCustomers");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Properties
        builder.Property(c => c.DiscountRuleId)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .IsRequired();

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
        builder.HasOne(c => c.DiscountRule)
            .WithMany(r => r.Customers)
            .HasForeignKey(c => c.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.DiscountRuleId);

        builder.HasIndex(c => c.CustomerId);

        // Composite unique index to prevent duplicates
        builder.HasIndex(c => new { c.DiscountRuleId, c.CustomerId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.IsDeleted);
    }
}
