using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(sc => sc.Id);

        // Properties
        builder.Property(sc => sc.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sc => sc.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sc => sc.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sc => sc.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sc => sc.IsEditable)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(sc => sc.CreatedAt)
            .IsRequired();

        builder.Property(sc => sc.CreatedBy)
            .HasMaxLength(100);

        builder.Property(sc => sc.UpdatedAt);

        builder.Property(sc => sc.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(sc => sc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sc => sc.DeletedAt);

        builder.Property(sc => sc.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(sc => sc.Key)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(sc => sc.Category);

        builder.HasIndex(sc => sc.IsDeleted);
    }
}
