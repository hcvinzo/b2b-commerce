using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.ParentCategoryId)
            .IsRequired(false);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Slug for URL-friendly identifier
        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(200);

        // Default ProductType FK
        builder.Property(c => c.DefaultProductTypeId);

        // Audit properties
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100);

        // Self-referential relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // DefaultProductType relationship
        builder.HasOne(c => c.DefaultProductType)
            .WithMany()
            .HasForeignKey(c => c.DefaultProductTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProductCategories relationship is configured in ProductCategoryConfiguration

        // Indexes
        builder.HasIndex(c => c.ParentCategoryId);

        builder.HasIndex(c => c.DisplayOrder);

        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.IsDeleted);

        builder.HasIndex(c => c.Name);

        // Unique slug index
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.DefaultProductTypeId);
    }
}
