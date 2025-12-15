using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");

        builder.HasKey(a => a.Id);

        // Global soft delete filter
        builder.HasQueryFilter(a => !a.IsDeleted);

        // Properties
        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.NameEn)
            .HasMaxLength(200);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.Unit)
            .HasMaxLength(50);

        builder.Property(a => a.IsFilterable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.IsVisibleOnProductPage)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit properties
        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100);

        builder.Property(a => a.UpdatedAt);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DeletedAt);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(100);

        // ExternalEntity properties
        builder.Property(a => a.ExternalId)
            .HasMaxLength(100);

        builder.Property(a => a.ExternalCode)
            .HasMaxLength(100);

        builder.Property(a => a.LastSyncedAt);

        // Relationships
        builder.HasMany(a => a.PredefinedValues)
            .WithOne(v => v.AttributeDefinition)
            .HasForeignKey(v => v.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Unique index on ExternalId (primary upsert key)
        builder.HasIndex(a => a.ExternalId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");

        // Non-unique index on ExternalCode (optional reference)
        builder.HasIndex(a => a.ExternalCode)
            .HasFilter("\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

        builder.HasIndex(a => a.IsFilterable);

        builder.HasIndex(a => a.DisplayOrder);

        builder.HasIndex(a => a.IsDeleted);
    }
}
