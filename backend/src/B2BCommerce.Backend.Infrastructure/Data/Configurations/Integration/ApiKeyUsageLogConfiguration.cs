using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations.Integration;

/// <summary>
/// EF Core configuration for ApiKeyUsageLog entity
/// </summary>
public class ApiKeyUsageLogConfiguration : IEntityTypeConfiguration<ApiKeyUsageLog>
{
    public void Configure(EntityTypeBuilder<ApiKeyUsageLog> builder)
    {
        builder.ToTable("ApiKeyUsageLogs");

        builder.HasKey(x => x.Id);

        // Use long for high volume
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Endpoint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.HttpMethod)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(x => x.ApiKeyId);
        builder.HasIndex(x => x.RequestTimestamp);
        builder.HasIndex(x => new { x.ApiKeyId, x.RequestTimestamp });

        // Relationship (no navigation from ApiKey for performance)
        builder.HasOne(x => x.ApiKey)
            .WithMany()
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
