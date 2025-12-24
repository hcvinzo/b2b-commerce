using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductRelationConfiguration : IEntityTypeConfiguration<ProductRelation>
{
    public void Configure(EntityTypeBuilder<ProductRelation> builder)
    {
        builder.ToTable("ProductRelations");

        builder.HasKey(pr => pr.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pr => !pr.IsDeleted);

        // Properties
        builder.Property(pr => pr.SourceProductId)
            .IsRequired();

        builder.Property(pr => pr.RelatedProductId)
            .IsRequired();

        builder.Property(pr => pr.RelationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pr => pr.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit properties
        builder.Property(pr => pr.CreatedAt)
            .IsRequired();

        builder.Property(pr => pr.CreatedBy)
            .HasMaxLength(100);

        builder.Property(pr => pr.UpdatedAt);

        builder.Property(pr => pr.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(pr => pr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pr => pr.DeletedAt);

        builder.Property(pr => pr.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(pr => pr.SourceProduct)
            .WithMany(p => p.SourceRelations)
            .HasForeignKey(pr => pr.SourceProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.RelatedProduct)
            .WithMany(p => p.TargetRelations)
            .HasForeignKey(pr => pr.RelatedProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique composite index: SourceProduct + RelatedProduct + RelationType
        // Prevents duplicate relations of the same type between two products
        builder.HasIndex(pr => new { pr.SourceProductId, pr.RelatedProductId, pr.RelationType })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Index for querying by source product and type
        builder.HasIndex(pr => new { pr.SourceProductId, pr.RelationType });

        // Index for querying by related product and type (for bidirectional lookup)
        builder.HasIndex(pr => new { pr.RelatedProductId, pr.RelationType });

        builder.HasIndex(pr => pr.DisplayOrder);

        builder.HasIndex(pr => pr.IsDeleted);
    }
}
