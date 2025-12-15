using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("ProductAttributeValues");

        builder.HasKey(pav => pav.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pav => !pav.IsDeleted);

        // Properties
        builder.Property(pav => pav.ProductId)
            .IsRequired();

        builder.Property(pav => pav.AttributeDefinitionId)
            .IsRequired();

        builder.Property(pav => pav.TextValue)
            .HasMaxLength(2000);

        builder.Property(pav => pav.NumericValue)
            .HasColumnType("decimal(18,4)");

        builder.Property(pav => pav.AttributeValueId);

        builder.Property(pav => pav.BooleanValue);

        builder.Property(pav => pav.DateValue);

        // Store MultiSelectValueIds as JSONB
        builder.Property(pav => pav.MultiSelectValueIds)
            .HasColumnType("jsonb");

        // Audit properties
        builder.Property(pav => pav.CreatedAt)
            .IsRequired();

        builder.Property(pav => pav.CreatedBy)
            .HasMaxLength(100);

        builder.Property(pav => pav.UpdatedAt);

        builder.Property(pav => pav.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(pav => pav.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pav => pav.DeletedAt);

        builder.Property(pav => pav.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(pav => pav.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(pav => pav.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pav => pav.AttributeDefinition)
            .WithMany()
            .HasForeignKey(pav => pav.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pav => pav.SelectedValue)
            .WithMany()
            .HasForeignKey(pav => pav.AttributeValueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes - Unique composite index for Product + AttributeDefinition
        builder.HasIndex(pav => new { pav.ProductId, pav.AttributeDefinitionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(pav => pav.AttributeDefinitionId);

        builder.HasIndex(pav => pav.AttributeValueId);

        builder.HasIndex(pav => pav.IsDeleted);
    }
}
