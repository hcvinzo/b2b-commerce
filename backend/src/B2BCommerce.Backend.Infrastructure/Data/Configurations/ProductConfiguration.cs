using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Global soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.BrandId)
            .IsRequired(false);

        // Money value objects as owned types - List Price
        builder.OwnsOne(p => p.ListPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ListPriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ListPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Tier Prices
        builder.OwnsOne(p => p.Tier1Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tier1PriceAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Tier1PriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.Tier2Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tier2PriceAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Tier2PriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.Tier3Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tier3PriceAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Tier3PriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.Tier4Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tier4PriceAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Tier4PriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.Tier5Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tier5PriceAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Tier5PriceCurrency")
                .HasMaxLength(3);
        });

        // Stock properties
        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.MinimumOrderQuantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsSerialTracked)
            .IsRequired()
            .HasDefaultValue(false);

        // Tax
        builder.Property(p => p.TaxRate)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        // Images
        builder.Property(p => p.MainImageUrl)
            .HasMaxLength(500);

        // Store ImageUrls as JSON
        builder.Property(p => p.ImageUrls)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");

        // Store Specifications as JSON
        builder.Property(p => p.Specifications)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("jsonb");

        // Dimensions and weight
        builder.Property(p => p.Weight)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Length)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Width)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Height)
            .HasColumnType("decimal(18,2)");

        // Audit properties
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedAt);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        // ProductType FK (optional for backward compatibility)
        builder.Property(p => p.ProductTypeId);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProductType relationship is configured in ProductTypeConfiguration

        // ProductCategories relationship is configured in ProductCategoryConfiguration

        // AttributeValues relationship is configured in ProductAttributeValueConfiguration

        // Indexes
        builder.HasIndex(p => p.SKU)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.CategoryId);

        builder.HasIndex(p => p.BrandId);

        builder.HasIndex(p => p.ProductTypeId);

        builder.HasIndex(p => p.IsActive);

        builder.HasIndex(p => p.IsDeleted);
    }
}
