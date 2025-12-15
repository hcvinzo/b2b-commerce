using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);

        // Global soft delete filter
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.UpdatedAt);

        builder.Property(b => b.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAt);

        builder.Property(b => b.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(b => b.Name);

        builder.HasIndex(b => b.IsActive);

        builder.HasIndex(b => b.IsDeleted);
    }
}
