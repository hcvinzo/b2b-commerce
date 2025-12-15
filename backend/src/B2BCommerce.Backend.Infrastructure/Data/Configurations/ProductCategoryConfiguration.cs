using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(pc => pc.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pc => !pc.IsDeleted);

        // Properties
        builder.Property(pc => pc.ProductId)
            .IsRequired();

        builder.Property(pc => pc.CategoryId)
            .IsRequired();

        builder.Property(pc => pc.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pc => pc.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit properties
        builder.Property(pc => pc.CreatedAt)
            .IsRequired();

        builder.Property(pc => pc.CreatedBy)
            .HasMaxLength(100);

        builder.Property(pc => pc.UpdatedAt);

        builder.Property(pc => pc.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(pc => pc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pc => pc.DeletedAt);

        builder.Property(pc => pc.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes - Unique composite index for Product + Category
        builder.HasIndex(pc => new { pc.ProductId, pc.CategoryId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Index for finding primary category
        builder.HasIndex(pc => new { pc.ProductId, pc.IsPrimary })
            .HasFilter("\"IsPrimary\" = true AND \"IsDeleted\" = false");

        builder.HasIndex(pc => pc.CategoryId);

        builder.HasIndex(pc => pc.DisplayOrder);

        builder.HasIndex(pc => pc.IsDeleted);
    }
}
