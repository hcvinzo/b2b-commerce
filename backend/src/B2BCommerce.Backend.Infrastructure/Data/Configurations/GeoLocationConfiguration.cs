using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class GeoLocationConfiguration : IEntityTypeConfiguration<GeoLocation>
{
    public void Configure(EntityTypeBuilder<GeoLocation> builder)
    {
        builder.ToTable("GeoLocations");

        builder.HasKey(e => e.Id);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        // ExternalEntity properties
        builder.Property(e => e.ExternalId)
            .HasMaxLength(100);

        builder.Property(e => e.ExternalCode)
            .HasMaxLength(100);

        // Properties
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Latitude)
            .HasPrecision(9, 6);

        builder.Property(e => e.Longitude)
            .HasPrecision(9, 6);

        builder.Property(e => e.Path)
            .HasMaxLength(500);

        builder.Property(e => e.PathName)
            .HasMaxLength(1000);

        builder.Property(e => e.Depth)
            .HasDefaultValue(0);

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(e => e.Type)
            .WithMany(t => t.Locations)
            .HasForeignKey(e => e.GeoLocationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.ExternalId)
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL AND \"IsDeleted\" = false");

        builder.HasIndex(e => e.Code);

        builder.HasIndex(e => e.ParentId);

        builder.HasIndex(e => e.GeoLocationTypeId);

        builder.HasIndex(e => e.Path);

        builder.HasIndex(e => e.IsDeleted);
    }
}
