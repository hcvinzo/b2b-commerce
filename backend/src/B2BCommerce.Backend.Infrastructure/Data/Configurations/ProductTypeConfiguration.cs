using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductTypes");

        builder.HasKey(pt => pt.Id);

        // Global soft delete filter
        builder.HasQueryFilter(pt => !pt.IsDeleted);

        // Properties
        builder.Property(pt => pt.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pt => pt.Description)
            .HasMaxLength(1000);

        builder.Property(pt => pt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(pt => pt.CreatedAt)
            .IsRequired();

        builder.Property(pt => pt.CreatedBy)
            .HasMaxLength(100);

        builder.Property(pt => pt.UpdatedAt);

        builder.Property(pt => pt.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(pt => pt.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pt => pt.DeletedAt);

        builder.Property(pt => pt.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(pt => pt.Attributes)
            .WithOne(pta => pta.ProductType)
            .HasForeignKey(pta => pta.ProductTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pt => pt.Products)
            .WithOne(p => p.ProductType)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pt => pt.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(pt => pt.IsActive);

        builder.HasIndex(pt => pt.IsDeleted);
    }
}
