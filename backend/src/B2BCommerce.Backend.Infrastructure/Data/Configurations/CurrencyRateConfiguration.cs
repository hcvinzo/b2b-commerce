using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("CurrencyRates");

        builder.HasKey(cr => cr.Id);

        // Global soft delete filter
        builder.HasQueryFilter(cr => !cr.IsDeleted);

        // Properties
        builder.Property(cr => cr.FromCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(cr => cr.ToCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(cr => cr.Rate)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(cr => cr.EffectiveDate)
            .IsRequired();

        builder.Property(cr => cr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(cr => cr.CreatedAt)
            .IsRequired();

        builder.Property(cr => cr.CreatedBy)
            .HasMaxLength(100);

        builder.Property(cr => cr.UpdatedAt);

        builder.Property(cr => cr.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(cr => cr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cr => cr.DeletedAt);

        builder.Property(cr => cr.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(cr => new { cr.FromCurrency, cr.ToCurrency, cr.EffectiveDate })
            .HasFilter("\"IsDeleted\" = false AND \"IsActive\" = true");

        builder.HasIndex(cr => cr.FromCurrency);

        builder.HasIndex(cr => cr.ToCurrency);

        builder.HasIndex(cr => cr.IsActive);

        builder.HasIndex(cr => cr.EffectiveDate);

        builder.HasIndex(cr => cr.IsDeleted);
    }
}
