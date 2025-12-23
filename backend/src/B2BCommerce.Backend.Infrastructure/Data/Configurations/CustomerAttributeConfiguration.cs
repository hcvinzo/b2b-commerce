using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CustomerAttributeConfiguration : IEntityTypeConfiguration<CustomerAttribute>
{
    public void Configure(EntityTypeBuilder<CustomerAttribute> builder)
    {
        builder.ToTable("CustomerAttributes");

        builder.HasKey(ca => ca.Id);

        // Global soft delete filter
        builder.HasQueryFilter(ca => !ca.IsDeleted);

        // Properties
        builder.Property(ca => ca.CustomerId)
            .IsRequired();

        builder.Property(ca => ca.AttributeType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ca => ca.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ca => ca.JsonData)
            .IsRequired()
            .HasColumnType("jsonb");

        // Audit properties
        builder.Property(ca => ca.CreatedAt)
            .IsRequired();

        builder.Property(ca => ca.CreatedBy)
            .HasMaxLength(100);

        builder.Property(ca => ca.UpdatedAt);

        builder.Property(ca => ca.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(ca => ca.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ca => ca.DeletedAt);

        builder.Property(ca => ca.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.Attributes)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ca => ca.CustomerId);

        builder.HasIndex(ca => ca.AttributeType);

        builder.HasIndex(ca => new { ca.CustomerId, ca.AttributeType });

        builder.HasIndex(ca => ca.IsDeleted);
    }
}
