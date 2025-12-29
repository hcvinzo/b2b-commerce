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

        builder.Property(ca => ca.AttributeDefinitionId)
            .IsRequired();

        builder.Property(ca => ca.Value)
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

        builder.HasOne(ca => ca.AttributeDefinition)
            .WithMany()
            .HasForeignKey(ca => ca.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ca => ca.CustomerId);

        builder.HasIndex(ca => ca.AttributeDefinitionId);

        builder.HasIndex(ca => new { ca.CustomerId, ca.AttributeDefinitionId });

        builder.HasIndex(ca => ca.IsDeleted);
    }
}
