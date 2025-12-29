using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("CustomerContacts");

        builder.HasKey(e => e.Id);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Properties
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Position)
            .HasMaxLength(100);

        builder.Property(e => e.Gender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Gender.Unknown);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.PhoneExt)
            .HasMaxLength(10);

        builder.Property(e => e.Gsm)
            .HasMaxLength(20);

        builder.Property(e => e.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Contacts)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.CustomerId);

        builder.HasIndex(e => e.Email);

        builder.HasIndex(e => new { e.CustomerId, e.IsPrimary })
            .HasFilter("\"IsPrimary\" = true AND \"IsDeleted\" = false");

        builder.HasIndex(e => e.IsActive);

        builder.HasIndex(e => e.IsDeleted);
    }
}
