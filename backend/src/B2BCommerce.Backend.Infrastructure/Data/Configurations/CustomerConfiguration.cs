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

        builder.Property(c => c.TradeName)
            .IsRequired()
            .HasMaxLength(100);

        // TaxNumber value object - simple wrapper
        builder.Property(c => c.TaxNumber)
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.TaxNumber(v))
            .HasColumnName("TaxNumber")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.TaxOffice)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.MersisNo)
            .HasMaxLength(20);

        builder.Property(c => c.IdentityNo)
            .HasColumnName("IdentityNo")
            .HasMaxLength(11);

        builder.Property(c => c.TradeRegistryNo)
            .HasMaxLength(50);

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

        // MobilePhone value object - nullable
        builder.Property(c => c.MobilePhone)
            .HasConversion(
                v => v != null ? v.Value : null,
                v => v != null ? new Domain.ValueObjects.PhoneNumber(v) : null)
            .HasColumnName("MobilePhone")
            .HasMaxLength(20);

        builder.Property(c => c.Fax)
            .HasMaxLength(20);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

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

        // Addresses relationship - handled by CustomerAddressConfiguration
        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Settings
        builder.Property(c => c.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("TRY");

        builder.Property(c => c.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("tr");

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
