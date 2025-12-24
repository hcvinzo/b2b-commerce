using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductCollectionConfiguration : IEntityTypeConfiguration<ProductCollection>
{
    public void Configure(EntityTypeBuilder<ProductCollection> builder)
    {
        builder.ToTable("ProductCollections");

        builder.HasKey(pc => pc.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pc => !pc.IsDeleted);

        // Properties
        builder.Property(pc => pc.CollectionId)
            .IsRequired();

        builder.Property(pc => pc.ProductId)
            .IsRequired();

        builder.Property(pc => pc.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pc => pc.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

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
        builder.HasOne(pc => pc.Collection)
            .WithMany(c => c.ProductCollections)
            .HasForeignKey(pc => pc.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCollections)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes - Unique composite index for Collection + Product
        builder.HasIndex(pc => new { pc.CollectionId, pc.ProductId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Index for ordering products within a collection
        builder.HasIndex(pc => new { pc.CollectionId, pc.DisplayOrder });

        // Index for finding featured products in a collection
        builder.HasIndex(pc => new { pc.CollectionId, pc.IsFeatured })
            .HasFilter("\"IsFeatured\" = true AND \"IsDeleted\" = false");

        builder.HasIndex(pc => pc.ProductId);

        builder.HasIndex(pc => pc.IsDeleted);
    }
}
