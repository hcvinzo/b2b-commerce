using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductTypeAttributeConfiguration : IEntityTypeConfiguration<ProductTypeAttribute>
{
    public void Configure(EntityTypeBuilder<ProductTypeAttribute> builder)
    {
        builder.ToTable("ProductTypeAttributes");

        builder.HasKey(pta => pta.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pta => !pta.IsDeleted);

        // Properties
        builder.Property(pta => pta.ProductTypeId)
            .IsRequired();

        builder.Property(pta => pta.AttributeDefinitionId)
            .IsRequired();

        builder.Property(pta => pta.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pta => pta.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit properties
        builder.Property(pta => pta.CreatedAt)
            .IsRequired();

        builder.Property(pta => pta.CreatedBy)
            .HasMaxLength(100);

        builder.Property(pta => pta.UpdatedAt);

        builder.Property(pta => pta.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(pta => pta.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pta => pta.DeletedAt);

        builder.Property(pta => pta.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(pta => pta.AttributeDefinition)
            .WithMany()
            .HasForeignKey(pta => pta.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - Unique composite index for ProductType + AttributeDefinition
        builder.HasIndex(pta => new { pta.ProductTypeId, pta.AttributeDefinitionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(pta => pta.DisplayOrder);

        builder.HasIndex(pta => pta.IsDeleted);
    }
}
