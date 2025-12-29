using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class GeoLocationTypeConfiguration : IEntityTypeConfiguration<GeoLocationType>
{
    public void Configure(EntityTypeBuilder<GeoLocationType> builder)
    {
        builder.ToTable("GeoLocationTypes");

        builder.HasKey(e => e.Id);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DisplayOrder)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => e.IsDeleted);
    }
}
