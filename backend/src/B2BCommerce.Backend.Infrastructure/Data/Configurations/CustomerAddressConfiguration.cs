using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(ca => ca.Id);

        // Global soft delete filter
        builder.HasQueryFilter(ca => !ca.IsDeleted);

        // Properties
        builder.Property(ca => ca.CustomerId)
            .IsRequired();

        builder.Property(ca => ca.AddressType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ca => ca.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ca => ca.FullName)
            .HasMaxLength(200);

        builder.Property(ca => ca.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ca => ca.GeoLocationId);

        builder.Property(ca => ca.PostalCode)
            .HasMaxLength(20);

        builder.Property(ca => ca.Phone)
            .HasMaxLength(20);

        builder.Property(ca => ca.PhoneExt)
            .HasMaxLength(10);

        builder.Property(ca => ca.Gsm)
            .HasMaxLength(20);

        builder.Property(ca => ca.TaxNo)
            .HasMaxLength(20);

        builder.Property(ca => ca.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ca => ca.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit properties
        builder.Property(ca => ca.CreatedAt)
            .IsRequired();

        builder.Property(ca => ca.CreatedBy)
            .HasMaxLength(100);

        builder.Property(ca => ca.UpdatedAt);

        builder.Property(ca => ca.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(ca => ca.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ca => ca.DeletedAt);

        builder.Property(ca => ca.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.GeoLocation)
            .WithMany()
            .HasForeignKey(ca => ca.GeoLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(ca => ca.CustomerId);

        builder.HasIndex(ca => ca.AddressType);

        builder.HasIndex(ca => ca.GeoLocationId);

        builder.HasIndex(ca => ca.IsDefault);

        builder.HasIndex(ca => ca.IsActive);

        builder.HasIndex(ca => ca.IsDeleted);
    }
}
