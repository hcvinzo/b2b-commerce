using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CollectionFilterConfiguration : IEntityTypeConfiguration<CollectionFilter>
{
    public void Configure(EntityTypeBuilder<CollectionFilter> builder)
    {
        builder.ToTable("CollectionFilters");

        builder.HasKey(cf => cf.Id);

        // Global soft delete filter
        builder.HasQueryFilter(cf => !cf.IsDeleted);

        // Properties
        builder.Property(cf => cf.CollectionId)
            .IsRequired();

        // Store lists as JSON columns with explicit conversion
        builder.Property(cf => cf.CategoryIds)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());

        builder.Property(cf => cf.BrandIds)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());

        builder.Property(cf => cf.ProductTypeIds)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Guid>());

        builder.Property(cf => cf.MinPrice)
            .HasPrecision(18, 4);

        builder.Property(cf => cf.MaxPrice)
            .HasPrecision(18, 4);

        // Audit properties
        builder.Property(cf => cf.CreatedAt)
            .IsRequired();

        builder.Property(cf => cf.CreatedBy)
            .HasMaxLength(100);

        builder.Property(cf => cf.UpdatedAt);

        builder.Property(cf => cf.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(cf => cf.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cf => cf.DeletedAt);

        builder.Property(cf => cf.DeletedBy)
            .HasMaxLength(100);

        // Relationship - one-to-one with Collection
        builder.HasOne(cf => cf.Collection)
            .WithOne(c => c.Filter)
            .HasForeignKey<CollectionFilter>(cf => cf.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cf => cf.CollectionId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(cf => cf.IsDeleted);
    }
}
