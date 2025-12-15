using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");

        builder.HasKey(v => v.Id);

        // Global soft delete filter
        builder.HasQueryFilter(v => !v.IsDeleted);

        // Properties
        builder.Property(v => v.AttributeDefinitionId)
            .IsRequired();

        builder.Property(v => v.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.DisplayText)
            .HasMaxLength(500);

        builder.Property(v => v.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit properties
        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.CreatedBy)
            .HasMaxLength(100);

        builder.Property(v => v.UpdatedAt);

        builder.Property(v => v.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(v => v.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.DeletedAt);

        builder.Property(v => v.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(v => v.AttributeDefinitionId);

        builder.HasIndex(v => new { v.AttributeDefinitionId, v.Value })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(v => v.DisplayOrder);

        builder.HasIndex(v => v.IsDeleted);
    }
}
