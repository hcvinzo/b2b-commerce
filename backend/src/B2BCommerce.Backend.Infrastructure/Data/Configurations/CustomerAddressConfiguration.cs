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

        builder.Property(ca => ca.AddressTitle)
            .IsRequired()
            .HasMaxLength(100);

        // Address value object as owned type
        builder.OwnsOne(ca => ca.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .HasColumnName("City")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("Country")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .IsRequired()
                .HasMaxLength(20);
        });

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

        // Indexes
        builder.HasIndex(ca => ca.CustomerId);

        builder.HasIndex(ca => ca.IsDefault);

        builder.HasIndex(ca => ca.IsActive);

        builder.HasIndex(ca => ca.IsDeleted);
    }
}
