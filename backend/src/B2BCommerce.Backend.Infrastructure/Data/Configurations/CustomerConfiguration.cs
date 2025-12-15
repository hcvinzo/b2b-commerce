using B2BCommerce.Backend.Domain.Entities;
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

        // Properties
        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        // TaxNumber value object - simple wrapper
        builder.Property(c => c.TaxNumber)
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.TaxNumber(v))
            .HasColumnName("TaxNumber")
            .IsRequired()
            .HasMaxLength(20);

        // Email value object - simple wrapper
        builder.Property(c => c.Email)
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.Email(v))
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(200);

        // PhoneNumber value object - simple wrapper
        builder.Property(c => c.Phone)
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.PhoneNumber(v))
            .HasColumnName("Phone")
            .IsRequired()
            .HasMaxLength(20);

        // Enum properties
        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.PriceTier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Money value objects - Credit Limit
        builder.OwnsOne(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CreditLimitAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("CreditLimitCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money value objects - Used Credit
        builder.OwnsOne(c => c.UsedCredit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UsedCreditAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UsedCreditCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Approval properties
        builder.Property(c => c.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.ApprovedAt);

        builder.Property(c => c.ApprovedBy)
            .HasMaxLength(100);

        // Contact information
        builder.Property(c => c.ContactPersonName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactPersonTitle)
            .IsRequired()
            .HasMaxLength(100);

        // Address value objects as owned types - Billing Address
        builder.OwnsOne(c => c.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("BillingStreet")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .HasColumnName("BillingCity")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("BillingState")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("BillingCountry")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("BillingPostalCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        // Address value objects as owned types - Shipping Address
        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .IsRequired()
                .HasMaxLength(500);

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("ShippingState")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        // Settings
        builder.Property(c => c.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(c => c.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.TaxNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.CompanyName);

        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => c.IsApproved);

        builder.HasIndex(c => c.IsDeleted);
    }
}
