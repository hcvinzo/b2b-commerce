using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for CustomerDocument entity
/// </summary>
public class CustomerDocumentConfiguration : IEntityTypeConfiguration<CustomerDocument>
{
    public void Configure(EntityTypeBuilder<CustomerDocument> builder)
    {
        builder.ToTable("CustomerDocuments");

        builder.HasKey(cd => cd.Id);

        // Global soft delete filter
        builder.HasQueryFilter(cd => !cd.IsDeleted);

        // Properties
        builder.Property(cd => cd.CustomerId)
            .IsRequired();

        builder.Property(cd => cd.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(cd => cd.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cd => cd.FileType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cd => cd.ContentUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(cd => cd.FileSize)
            .IsRequired();

        // Audit properties
        builder.Property(cd => cd.CreatedAt)
            .IsRequired();

        builder.Property(cd => cd.CreatedBy)
            .HasMaxLength(100);

        builder.Property(cd => cd.UpdatedAt);

        builder.Property(cd => cd.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(cd => cd.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cd => cd.DeletedAt);

        builder.Property(cd => cd.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(cd => cd.Customer)
            .WithMany()
            .HasForeignKey(cd => cd.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cd => cd.CustomerId);

        builder.HasIndex(cd => cd.DocumentType);

        builder.HasIndex(cd => new { cd.CustomerId, cd.DocumentType });

        builder.HasIndex(cd => cd.IsDeleted);
    }
}
