using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        // Global soft delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // ExternalEntity properties
        builder.Property(c => c.ExternalId)
            .HasMaxLength(100);

        builder.Property(c => c.ExternalCode)
            .HasMaxLength(100);

        // Properties
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.TaxOffice)
            .HasMaxLength(200);

        builder.Property(c => c.TaxNo)
            .HasMaxLength(20);

        builder.Property(c => c.EstablishmentYear);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CustomerStatus.Pending);

        builder.Property(c => c.UserId);

        builder.Property(c => c.DocumentUrls)
            .HasColumnType("jsonb");

        // Audit properties (inherited from BaseEntity, explicitly configured)
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

        // Relationships
        builder.HasMany(c => c.Contacts)
            .WithOne(ct => ct.Customer)
            .HasForeignKey(ct => ct.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Attributes)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.ExternalId)
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL AND \"IsDeleted\" = false");

        builder.HasIndex(c => c.TaxNo)
            .IsUnique()
            .HasFilter("\"TaxNo\" IS NOT NULL AND \"IsDeleted\" = false");

        builder.HasIndex(c => c.Title);

        builder.HasIndex(c => c.Status);

        builder.HasIndex(c => c.UserId);

        builder.HasIndex(c => c.IsDeleted);
    }
}
