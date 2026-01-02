using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Properties
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.DecimalPlaces)
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(c => c.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

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

        // Indexes
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.IsDefault)
            .HasFilter("\"IsDeleted\" = false AND \"IsDefault\" = true");

        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.DisplayOrder);

        builder.HasIndex(c => c.IsDeleted);
    }
}
