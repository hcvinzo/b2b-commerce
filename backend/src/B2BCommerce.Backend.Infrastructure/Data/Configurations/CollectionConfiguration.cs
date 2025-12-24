using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.StartDate);

        builder.Property(c => c.EndDate);

        // Ignore computed property
        builder.Ignore(c => c.IsCurrentlyActive);

        // External entity fields (from ExternalEntity base class)
        builder.Property(c => c.ExternalCode)
            .HasMaxLength(100);

        builder.Property(c => c.ExternalId)
            .HasMaxLength(100);

        builder.Property(c => c.LastSyncedAt);

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

        // Relationships - configure backing field for private collection
        builder.Navigation(c => c.ProductCollections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.ProductCollections)
            .WithOne(pc => pc.Collection)
            .HasForeignKey(pc => pc.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Filter)
            .WithOne(f => f.Collection)
            .HasForeignKey<CollectionFilter>(f => f.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.Type);

        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.IsFeatured);

        builder.HasIndex(c => c.DisplayOrder);

        builder.HasIndex(c => c.IsDeleted);

        builder.HasIndex(c => new { c.StartDate, c.EndDate });

        // External ID unique index
        builder.HasIndex(c => c.ExternalId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");

        // External code index
        builder.HasIndex(c => c.ExternalCode)
            .HasFilter("\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

        builder.HasIndex(c => c.LastSyncedAt);
    }
}
